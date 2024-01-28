CREATE TABLE files
(
    id SERIAL PRIMARY KEY,
    session_id TEXT     NOT NULL,
    filename   TEXT     NOT NULL,
    status SMALLINT NOT NULL,
    html_file BYTEA,
    converted_pdf_file BYTEA
);