CREATE TABLE knowledge_node (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    node_type_id UUID NOT NULL REFERENCES node_type(id),
    canonical_name VARCHAR(250) NOT NULL,
    description TEXT,
    created_utc TIMESTAMP NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
    modified_utc TIMESTAMP NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
    CONSTRAINT uq_knowledge_node_node_type_id_canonical_name UNIQUE (node_type_id, canonical_name)
);

CREATE TRIGGER trg_knowledge_node_set_modified_utc
BEFORE UPDATE ON knowledge_node
FOR EACH ROW
EXECUTE FUNCTION set_modified_utc();
