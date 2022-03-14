// Calling all the required packages
const express = require("express");
const bodyParser = require("body-parser");
const path = require("path");
const multer = require("multer");
const Hash = require('ipfs-only-hash')
const fs = require('fs');

const app = express();

// Configurations for "body-parser"
app.use(
    bodyParser.urlencoded({
        extended: true,
    })
);

// Configurations for setting up ejs engine &
// displaying static files from "public" folder
// app.set("view engine", "ejs");
app.use(express.static(__dirname + '/public'));

// Configuration for Multer
const multerStorage = multer.diskStorage({
    destination: (req, file, cb) => {
        cb(null, "./public/files/");
    },
    filename: (req, file, cb) => {
        cb(null, file.originalname);
    },
});

// Multer Filter
const multerFilter = (req, file, cb) => {
    if (file.mimetype.split("/")[1] === "json") {
        cb(null, true);
    } else {
        cb(new Error("Not a JSON file!!"), false);
    }
};

// Calling the "multer" Function
const upload = multer({
    storage: multerStorage,
    fileFilter: multerFilter,
});

// API Endpoint for uploading file
app.post("/api/hashFile", upload.single("myFile"), async (req, res) => {
    if (req.file === undefined) {
        res.status(400).json({
            status: "Bad request",
            message: "Missing file",
        });
        return;
    }
    
    console.log("/api/hashFile");
    console.log(req.file.path)
    
    const readableStream = fs.createReadStream(req.file.path);
    readableStream.on('error', function (error) {
        console.log(`error: ${error.message}`);
    })
    readableStream.on('data', async (chunk) => {
        // console.log(chunk);
        let hash = await Hash.of(chunk);
        console.log(hash)

        fs.unlink(req.file.path, (err) => {
            if (err) {
                console.error(err)
                return
            }
        })

        res.status(200).json({
            status: "success",
            message: "Hash calculated successfully!",
            hash: hash,
        });
    })
});

// API Endpoint to render HTML file
app.use("/", (req, res) => {
    res.status(200).render("index");
});

// Express server
module.exports = app;