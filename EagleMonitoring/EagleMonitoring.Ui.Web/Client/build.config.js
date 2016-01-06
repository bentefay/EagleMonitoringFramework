module.exports = {

    webPack: {
        entry: './app/main.tsx',
        output: {
            filename: 'app-bundle.js'
        },
        resolve: {
            extensions: ['', '.webpack.js', '.web.js', '.ts', '.tsx', '.js']
        },
        module: {
            loaders: [
                { test: /\.ts$/, loader: 'ts-loader' }
            ]
        }
    }
}