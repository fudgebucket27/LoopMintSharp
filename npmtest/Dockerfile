FROM node:16-alpine as BUILD_IMAGE
WORKDIR /usr/src/app

# Install app dependencies
# A wildcard is used to ensure both package.json AND package-lock.json are copied
# where available (npm@5+)
COPY package*.json ./

RUN npm ci --only-production
# If you are building your code for production
# RUN npm ci --only=production

# Bundle app source
COPY . .

RUN rm -rf node_modules/protobufjs/cli/
RUN rm -rf node_modules/protobufjs/src/
RUN rm -rf node_modules/protobufjs/dist/*.map

EXPOSE 3000
CMD [ "node", "server.js" ]