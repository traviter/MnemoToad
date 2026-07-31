CREATE TABLE country (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    iso_code TEXT NOT NULL UNIQUE,
    flag_image_url TEXT
);