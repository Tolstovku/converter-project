FROM postgres:latest

# For test purposes only
ENV POSTGRES_USER postgres_user
ENV POSTGRES_PASSWORD postgres_password
ENV POSTGRES_DB sample_database

EXPOSE 5432

COPY init-migration.sql /docker-entrypoint-initdb.d/