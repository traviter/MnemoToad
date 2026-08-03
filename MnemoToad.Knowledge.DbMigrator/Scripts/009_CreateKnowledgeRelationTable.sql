CREATE TABLE knowledge_relation (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source_node_id UUID NOT NULL,
    relationship_type_id UUID NOT NULL,
    target_node_id UUID NOT NULL,
    CONSTRAINT fk_knowledge_relation_source_node_id FOREIGN KEY (source_node_id) REFERENCES knowledge_node(id),
    CONSTRAINT fk_knowledge_relation_relationship_type_id FOREIGN KEY (relationship_type_id) REFERENCES relationship_type(id),
    CONSTRAINT fk_knowledge_relation_target_node_id FOREIGN KEY (target_node_id) REFERENCES knowledge_node(id),
    CONSTRAINT uq_knowledge_relation_source_type_target UNIQUE (source_node_id, relationship_type_id, target_node_id)
);
