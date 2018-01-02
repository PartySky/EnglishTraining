const path = require('path');

module.exports = {
    watch: false,
    devtool: 'source-map',
    entry: './frontend/main.module.ts',
    module: {
        rules: [{
                test: /\.ts?$/,
                use: 'ts-loader',
                exclude: /node_modules/
            },
            {
                test: /\.html$/,
                exclude: /node_modules/,
                loader: "html-loader?exportAsEs6Default"
            }
        ]
    },
    resolve: {
        extensions: [".tsx", ".ts", ".js", ".html"]
    },
    output: {
        filename: 'index.js',
        path: path.resolve(__dirname, 'wwwroot/scripts')
    }
};