<template>
    <div class="csv-upload">
      <h1>Upload CSV File</h1>
      <input type="file" @change="handleFileUpload" accept=".csv" />
      <div v-if="responseData">
        <h2>Upload Summary:</h2>
        <p><strong>Successful Count:</strong> {{ responseData.successfulCount }}</p>
        <p><strong>Failed Count:</strong> {{ responseData.failedCount }}</p>
  
        <div v-if="responseData.failureMessages.length">
          <h3>Failure Details:</h3>
          <table>
            <thead>
              <tr>
                <th>File Row</th>
                <th>Account ID</th>
                <th>Meter Reading Date</th>
                <th>Meter Reading Value</th>
                <th>Failure Message</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(failure, index) in responseData.failureMessages" :key="index">
                <td>{{ failure.csvFileRow }}</td>
                <td>{{ failure.accountId }}</td>
                <td>{{ formatDate(failure.meterReadingDateTime) }}</td>
                <td>{{ failure.meterReadingValue }}</td>
                <td>{{ failure.failureMessage }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div v-else-if="responseMessage">
        <h2>Response:</h2>
        <pre>{{ responseMessage }}</pre>
      </div>
    </div>
  </template>
  
  <script>
  import moment from 'moment';
  export default {
    data() {
      return {
        responseMessage: null,
        responseData: null,
      };
    },
    methods: {
      handleFileUpload(event) {
        const file = event.target.files[0];
        if (file && file.type === 'text/csv') {
          const formData = new FormData();
          formData.append('file', file);
  
          fetch('meter-reading-uploads', {
            method: 'POST',
            body: formData,
          })
            .then((response) => {
              if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
              }
              return response.json();
            })
            .then((data) => {
              this.responseData = data;
            })
            .catch((error) => {
              this.responseMessage = `Error: ${error.message}`;
            });
        } else {
          alert('Please upload a valid CSV file.');
        }
      },
      moment: function () {
        return moment;
      },
      formatDate(date) {
        return moment(date).format('YYYY-MM-DD HH:mm:ss');
      },
    },
  };
  </script>
  
  <style scoped>
  .csv-upload {
    max-width: 1000px;
    margin: 0 auto;
    text-align: center;
  }
  
  table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 20px;
  }
  
  th, td {
    border: 1px solid #ddd;
    padding: 8px;
    text-align: left;
  }
  
  th {
    background-color: #f4f4f4;
  }
  </style>