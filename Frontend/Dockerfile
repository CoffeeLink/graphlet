FROM node:22-alpine
WORKDIR /app
# copy the entire frontend project folder so nested files are preserved
COPY ./Graphlet/ ./

# expose the Vite default port and set it explicitly
ENV PORT=5173

# avoid trying to open a browser inside the container (xdg-open missing)
ENV BROWSER=none

RUN npm install --production=false

EXPOSE 5173

# bind Vite to all interfaces and don't open a browser
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--open", "false"]