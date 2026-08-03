CREATE FUNCTION set_modified_utc()
RETURNS TRIGGER AS $$
BEGIN
    NEW.modified_utc = (now() AT TIME ZONE 'utc');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
