CREATE TABLE attribute_type (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    description TEXT,
    CONSTRAINT uq_attribute_type_name UNIQUE (name)
);
