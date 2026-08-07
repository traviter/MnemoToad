CREATE TABLE knowledge_node_media (
    knowledge_node_id UUID NOT NULL,
    key VARCHAR(100) NOT NULL,
    media_asset_id UUID NOT NULL,
    alt_text VARCHAR(500) NOT NULL,
    metadata JSONB NOT NULL,
    CONSTRAINT pk_knowledge_node_media PRIMARY KEY (knowledge_node_id, key),
    CONSTRAINT fk_knowledge_node_media_knowledge_node_id FOREIGN KEY (knowledge_node_id) REFERENCES knowledge_node(id) ON DELETE CASCADE,
    CONSTRAINT uq_knowledge_node_media_media_asset_id UNIQUE (media_asset_id)
);
