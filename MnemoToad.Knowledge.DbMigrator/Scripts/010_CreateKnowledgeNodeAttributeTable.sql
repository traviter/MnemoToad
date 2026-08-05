CREATE TABLE knowledge_node_attribute (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    knowledge_node_id UUID NOT NULL,
    attribute_type_id UUID NOT NULL,
    value TEXT NOT NULL,
    CONSTRAINT fk_knowledge_node_attribute_knowledge_node_id FOREIGN KEY (knowledge_node_id) REFERENCES knowledge_node(id),
    CONSTRAINT fk_knowledge_node_attribute_attribute_type_id FOREIGN KEY (attribute_type_id) REFERENCES attribute_type(id),
    CONSTRAINT uq_knowledge_node_attribute_node_attribute_type UNIQUE (knowledge_node_id, attribute_type_id)
);
