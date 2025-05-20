const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  devServer: {
    proxy: 'http://localhost:5025',
    headers: {
      'Access-Control-Allow-Origin': '*'
    }
  }
})