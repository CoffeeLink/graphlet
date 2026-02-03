FROM node:22-alpine
WORKDIR /GraphletFrontend
COPY ./Graphlet/package*.json ./
COPY ./Graphlet/vite.config.ts ./
COPY ./Graphlet/tsconfig.json ./
COPY ./Graphlet/src/* ./src/
COPY ./Graphlet/public/* ./public/
COPY ./Graphlet/tsconfig.app.json ./
COPY ./Graphlet/tsconfig.node.json ./
RUN npm install
COPY . .
EXPOSE 5173
CMD ["npm", "run", "dev", "--", "--host"]