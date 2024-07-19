CREATE TABLE "Books" (
    "ID" SERIAL PRIMARY KEY,
    "Title" VARCHAR(255) NOT NULL,
    "Author" VARCHAR(255) NOT NULL,
    "ISBN" VARCHAR(13) NOT NULL,
    "PublishedDate" TIMESTAMP,
    "Price" DECIMAL CHECK ("Price" >= 0),
    "Quantity" INT
);

INSERT INTO "Books" ("Title", "Author", "ISBN", "PublishedDate", "Price", "Quantity") VALUES
('The Great Gatsby', 'F. Scott Fitzgerald', '9780743273565', '1925-04-10', 10.99, 100),
('1984', 'George Orwell', '9780451524935', '1949-06-08', 8.99, 200),
('To Kill a Mockingbird', 'Harper Lee', '9780060935467', '1960-07-11', 7.99, 150);
