# SQL reference practice

Start with SQL lesson 01 in the app and work in order. The examples share a small, synthetic shop database: four customers, six orders, and nine order lines. All names and transactions are invented.

- `seed.sql` creates the fixture and its primary, foreign, unique, and check constraints.
- `cases.json` contains the exact portable query for each of the 25 lessons, expected column names, and expected rows.
- `test_curriculum.py` runs the cases and extra checks for NULL, grouping/join mistakes, parameters, pagination, constraints, DML safety, and rollback.

Run from the repository root, using Python's built-in SQLite engine:

```sh
python -m unittest discover -s examples/sql -p 'test*.py' -v
```

The lessons teach portable relational ideas and identify SQL Server/T-SQL differences. SQLite results do not prove SQL Server syntax, isolation, locks, deadlocks, rowversion, query-plan behavior, or performance. Those engine-specific sections are source-reviewed and need a separate disposable SQL Server sandbox. Do not run learning mutations on a production database.

Before revealing a result, predict its columns and rows. Then change one condition or add a NULL/tie/missing child row and explain why the result changes.
