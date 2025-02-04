const fs = require('fs');
const https = require('https');
const express = require('express');
const options = {
  key: fs.readFileSync('../Ogar/keys/key.pem'),
  cert: fs.readFileSync('../Ogar/keys/cert.pem'),
};
const app = express();
app.use(express.static('web'));
https.createServer(options, app).listen(3000);
