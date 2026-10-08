"""Run with python -m unittest discover -s examples/sql -p 'test*.py' -v.
Only creates SQLite in-memory databases. Never connects to a real database.
SQL Server-only snippets are intentionally not claimed as executed.
"""
import json
import pathlib
import sqlite3
import unittest

HERE = pathlib.Path(__file__).resolve().parent
SEED = (HERE / 'seed.sql').read_text()
CASES = json.loads((HERE / 'cases.json').read_text())

class SqlCurriculumTests(unittest.TestCase):
    def setUp(self):
        self.db = sqlite3.connect(':memory:')
        self.db.execute('PRAGMA foreign_keys = ON')
        self.db.executescript(SEED)
        self.db.commit()

    def tearDown(self):
        self.db.close()

    def test_all_25_lesson_queries_and_headers(self):
        self.assertEqual(len(CASES), 25)
        for case in CASES:
            with self.subTest(lesson=case['id']):
                cursor = self.db.execute(case['query'], case['params'])
                actual = [list(row) for row in cursor.fetchall()]
                self.assertEqual(actual, case['expected'])
                self.assertEqual([d[0] for d in cursor.description], case['columns'])

    def test_fixture_counts_and_line_totals(self):
        self.assertEqual(self.db.execute('SELECT COUNT(*) FROM Customers').fetchone()[0], 4)
        self.assertEqual(self.db.execute('SELECT COUNT(*) FROM Orders').fetchone()[0], 6)
        self.assertEqual(self.db.execute('SELECT COUNT(*) FROM OrderItems').fetchone()[0], 9)
        bad = self.db.execute('''SELECT o.order_id FROM Orders o JOIN OrderItems i
            ON i.order_id = o.order_id GROUP BY o.order_id, o.total_cents
            HAVING SUM(i.quantity*i.unit_cents) <> o.total_cents''').fetchall()
        self.assertEqual(bad, [])

    def test_null_truth_and_count_examples(self):
        self.assertEqual(self.db.execute('SELECT name FROM Customers WHERE city = NULL').fetchall(), [])
        self.assertEqual(self.db.execute('SELECT name FROM Customers WHERE city IS NULL').fetchall(), [('Cy',)])
        self.assertEqual(self.db.execute("SELECT name FROM Customers WHERE city NOT IN ('Oslo', NULL)").fetchall(), [])
        self.assertEqual(self.db.execute('SELECT COUNT(*), COUNT(city) FROM Customers').fetchone(), (4, 3))
        self.assertEqual(self.db.execute('SELECT COUNT(*), SUM(total_cents), AVG(total_cents) FROM Orders WHERE order_id < 0').fetchone(), (0, None, None))

    def test_join_multiplication_warning(self):
        row = self.db.execute('''SELECT COUNT(*), SUM(o.total_cents),
            SUM(i.quantity*i.unit_cents) FROM Orders o JOIN OrderItems i
            ON i.order_id=o.order_id''').fetchone()
        self.assertEqual(row, (9, 10800, 7200))
        distinct = self.db.execute('SELECT SUM(DISTINCT total_cents) FROM Orders').fetchone()[0]
        self.assertEqual(distinct, 4000)

    def test_left_join_filter_placement(self):
        right_filter = self.db.execute("""SELECT DISTINCT c.name FROM Customers c
            LEFT JOIN Orders o ON o.customer_id=c.customer_id
            WHERE o.status='paid' ORDER BY c.name""").fetchall()
        self.assertEqual(right_filter, [('Ada',), ('Ben',)])
        absent = self.db.execute('''SELECT c.name FROM Customers c LEFT JOIN Orders o
            ON o.customer_id=c.customer_id WHERE o.order_id IS NULL''').fetchall()
        self.assertEqual(absent, [('Dee',)])

    def test_safe_dml_returns_to_original_state(self):
        self.db.execute('BEGIN TRANSACTION')
        self.db.execute("INSERT INTO Customers(customer_id,name,city) VALUES(99,'Toy','Oslo')")
        self.assertEqual(self.db.execute('SELECT COUNT(*) FROM Customers WHERE customer_id=99').fetchone()[0], 1)
        changed = self.db.execute("UPDATE Customers SET city='Paris' WHERE customer_id=99")
        self.assertEqual(changed.rowcount, 1)
        self.assertEqual(self.db.execute('SELECT customer_id,name,city FROM Customers WHERE customer_id=99').fetchone(), (99,'Toy','Paris'))
        deleted = self.db.execute('DELETE FROM Customers WHERE customer_id=99')
        self.assertEqual(deleted.rowcount, 1)
        self.assertEqual(self.db.execute('SELECT COUNT(*) FROM Customers WHERE customer_id=99').fetchone()[0], 0)
        self.db.rollback()
        self.assertEqual(self.db.execute('SELECT COUNT(*) FROM Customers').fetchone()[0], 4)

    def test_transaction_rollback(self):
        self.db.execute('BEGIN TRANSACTION')
        self.db.execute("UPDATE Orders SET status='paid' WHERE order_id=102")
        self.assertEqual(self.db.execute('SELECT status FROM Orders WHERE order_id=102').fetchone()[0], 'paid')
        self.db.rollback()
        self.assertEqual(self.db.execute('SELECT status FROM Orders WHERE order_id=102').fetchone()[0], 'pending')

    def test_conditional_update_is_repeat_safe(self):
        self.db.execute('BEGIN TRANSACTION')
        query = "UPDATE Orders SET status='paid' WHERE order_id=102 AND status='pending'"
        self.assertEqual(self.db.execute(query).rowcount, 1)
        self.assertEqual(self.db.execute(query).rowcount, 0)
        self.db.rollback()
        self.assertEqual(self.db.execute('SELECT status FROM Orders WHERE order_id=102').fetchone()[0], 'pending')

    def test_parameter_bound_input_remains_data(self):
        query = 'SELECT customer_id FROM Customers WHERE city=? ORDER BY customer_id'
        self.assertEqual(self.db.execute(query, ('Oslo',)).fetchall(), [(1,), (4,)])
        self.assertEqual(self.db.execute(query, ("Oslo' OR 1=1 --",)).fetchall(), [])
        self.assertEqual(self.db.execute('SELECT COUNT(*) FROM Customers').fetchone()[0], 4)

    def test_key_check_and_foreign_key_constraints(self):
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute("INSERT INTO Customers VALUES(1,'Duplicate','Oslo')")
        self.db.rollback()
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute("INSERT INTO Orders VALUES(999,999,'2026-01-01',100,'paid')")
        self.db.rollback()
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute("INSERT INTO Orders VALUES(999,1,'2026-01-01',-1,'paid')")
        self.db.rollback()
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute('DELETE FROM Customers WHERE customer_id=1')
        self.db.rollback()

    def test_index_plan_is_available_not_server_equivalence(self):
        self.db.execute('CREATE INDEX ix_orders_customer_date ON Orders(customer_id,ordered_on)')
        plan = self.db.execute("""EXPLAIN QUERY PLAN SELECT order_id FROM Orders
            WHERE customer_id=1 AND ordered_on >= '2026-01-01'
            AND ordered_on < '2026-02-01'""").fetchall()
        self.assertTrue(any('ix_orders_customer_date' in row[3] for row in plan), plan)
        self.assertEqual(self.db.execute("SELECT order_id FROM Orders WHERE customer_id=1 AND ordered_on>='2026-01-01' AND ordered_on<'2026-02-01' ORDER BY order_id").fetchall(), [(101,), (102,)])

    def test_keyset_paging_with_ties(self):
        rows = self.db.execute('SELECT order_id,ordered_on FROM Orders ORDER BY ordered_on DESC,order_id DESC LIMIT 1').fetchall()
        self.assertEqual(rows, [(106,'2026-02-02')])
        cursor_id, cursor_date = rows[-1]
        next_page = self.db.execute('''SELECT order_id,ordered_on FROM Orders
            WHERE ordered_on < ? OR (ordered_on = ? AND order_id < ?)
            ORDER BY ordered_on DESC,order_id DESC LIMIT 1''', (cursor_date,cursor_date,cursor_id)).fetchall()
        self.assertEqual(next_page, [(105,'2026-02-02')])

    def test_window_moving_sum(self):
        rows = self.db.execute('''SELECT order_id,
            SUM(total_cents) OVER (PARTITION BY customer_id ORDER BY ordered_on,order_id
            ROWS BETWEEN 1 PRECEDING AND CURRENT ROW)
            FROM Orders WHERE customer_id=1 ORDER BY ordered_on,order_id''').fetchall()
        self.assertEqual(rows, [(101,1200),(102,2000),(106,2000)])

    def test_union_all_keeps_duplicates(self):
        rows = self.db.execute('''SELECT city FROM Customers WHERE customer_id IN (1,2)
            UNION ALL SELECT city FROM Customers WHERE customer_id=4 ORDER BY city''').fetchall()
        self.assertEqual(rows, [('Oslo',), ('Oslo',), ('Paris',)])

if __name__ == '__main__':
    print('SQLite runtime:', sqlite3.sqlite_version)
    unittest.main(verbosity=2)
