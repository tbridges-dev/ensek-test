<template>
  <div id="app">
    <CsvUpload />
  </div>
</template>

<script>
import CsvUpload from './components/CsvUpload.vue';

export default {
  components: {
    CsvUpload,
  },
};
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>