CREATE TABLE relationship_type (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    inverse_name VARCHAR(100),
    description TEXT
);
