FROM rabbitmq:3-management

# Expose ports for RabbitMQ and the management plugin
EXPOSE 5672 15672

# Set up RabbitMQ to create a queue named "Files"
COPY definitions.json /etc/rabbitmq/definitions.json
COPY rabbitmq.conf /etc/rabbitmq/rabbitmq.conf
COPY init-rabbitmq.sh /usr/local/bin/
CMD ["init-rabbitmq.sh"]
ENTRYPOINT ["rabbitmq-server"]