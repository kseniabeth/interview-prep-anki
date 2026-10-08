-- Toy learning database only. Portable schema executed with SQLite.
-- In SQL Server use a new empty practice database. Prefer DATE for ordered_on
-- and NVARCHAR for international text in production; these VARCHAR ISO dates
-- deliberately keep the fixture identical across both engines.
CREATE TABLE Customers (
  customer_id INTEGER NOT NULL PRIMARY KEY,
  name VARCHAR(40) NOT NULL,
  city VARCHAR(40) NULL
);
CREATE TABLE Orders (
  order_id INTEGER NOT NULL PRIMARY KEY,
  customer_id INTEGER NOT NULL REFERENCES Customers(customer_id),
  ordered_on VARCHAR(10) NOT NULL,
  total_cents INTEGER NOT NULL CHECK (total_cents >= 0),
  status VARCHAR(10) NOT NULL CHECK (status IN ('paid','pending','canceled'))
);
CREATE TABLE OrderItems (
  item_id INTEGER NOT NULL PRIMARY KEY,
  order_id INTEGER NOT NULL REFERENCES Orders(order_id),
  product VARCHAR(40) NOT NULL,
  quantity INTEGER NOT NULL CHECK (quantity > 0),
  unit_cents INTEGER NOT NULL CHECK (unit_cents >= 0)
);
INSERT INTO Customers (customer_id,name,city) VALUES
 (1,'Ada','Oslo'),(2,'Ben','Paris'),(3,'Cy',NULL),(4,'Dee','Oslo');
INSERT INTO Orders (order_id,customer_id,ordered_on,total_cents,status) VALUES
 (101,1,'2026-01-05',1200,'paid'),
 (102,1,'2026-01-07',800,'pending'),
 (103,2,'2026-01-07',2000,'paid'),
 (104,3,'2026-02-01',800,'canceled'),
 (105,2,'2026-02-02',1200,'paid'),
 (106,1,'2026-02-02',1200,'paid');
INSERT INTO OrderItems (item_id,order_id,product,quantity,unit_cents) VALUES
 (1001,101,'Book',1,1000),(1002,101,'Pen',2,100),
 (1003,102,'Pad',1,800),(1004,103,'Book',2,1000),
 (1005,104,'Pad',1,800),(1006,105,'Book',1,1000),
 (1007,105,'Pen',2,100),(1008,106,'Book',1,1000),
 (1009,106,'Pen',2,100);
