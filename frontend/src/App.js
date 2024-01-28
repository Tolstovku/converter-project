import logo from './logo.svg';
import './App.css';
import {useEffect, useState} from "react";
import axios from 'axios';
import * as signalR from '@microsoft/signalr';


function App() {
  const [connection, setConnection] = useState(null);
  const [messages, setMessages] = useState([]);
  const [selectedFile, setSelectedFile] = useState(null);
  const [receivedFiles, setReceivedFiles] = useState([]);

  const enumStatusText = (status) => {
    switch (status) {
      case 0:
        return 'Uploaded 📥';
      case 1:
        return 'Converting ♻️';
      case 2:
        return 'Converted ✅';
      default:
        return 'Unknown';
    }
  };


  useEffect(() => {
    const authenticate = async () => {
      try {
        const response = await axios.get('http://localhost:5107/api/v1/authenticate', {
          withCredentials: true
        });

        console.log('Authentication successful:', response.data);
      } catch (error) {
        console.error('Error during authentication:', error);
      }
    };

    authenticate();
  }, []);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5107/status')
        .withAutomaticReconnect()
        .build();

    setConnection(newConnection);

    newConnection.start()
        .then(() => console.log('Connection to SignalR hub established'))
        .catch((error) => console.error('Error connecting to SignalR hub:', error));

    newConnection.on('ReceiveFilesInitial', (filesJson) => {
      const files = JSON.parse(filesJson);
      console.log('Received files:', files);
      setReceivedFiles(files);
    });

    newConnection.on('ReceiveFileUpdate', (fileJson) => {
      console.log("Receive file update: " + fileJson);
      const updatedFile = JSON.parse(fileJson);
      setReceivedFiles((prevFiles) =>
          prevFiles.map((file) =>
              file.Id === updatedFile.Id ? { ...file, Status: updatedFile.Status } : file
          )
      );
    });

    return () => {
      newConnection.stop()
          .then(() => console.log('Connection to SignalR hub stopped'))
          .catch((error) => console.error('Error stopping SignalR connection:', error));
    };
  }, []);

  useEffect(() => {
    if (connection) {
      connection.on('ReceiveMessage', (message) => {
        console.log("Received: " + message);
        setMessages((prevMessages) => [...prevMessages, message]);
      });
    }
  }, [connection]);

  const handleFileChange = (event) => {
    const file = event.target.files[0];
    setSelectedFile(file);
  };

  const handleDownload = async (fileId) => {
    console.log('Download file with Id:', fileId);
    const downloadLink = document.createElement('a');
    downloadLink.href = `http://localhost:5107/api/v1/file/${fileId}`;
    downloadLink.click();
  };
  

  const handleDelete = async (fileId) => {
    console.log('Delete file with Id:', fileId);
    try {
      await axios.delete(`http://localhost:5107/api/v1/file/${fileId}`, {
        withCredentials: true
      });

      setReceivedFiles((prevFiles) => prevFiles.filter((file) => file.Id !== fileId));
    } catch (error) {
      console.error('Error deleting file:', error);
    }
  };

  const handleUpload = async () => {
    if (selectedFile) {
      try {
        const formData = new FormData();
        formData.append('file', selectedFile);

        const response = await axios.post('http://localhost:5107/api/v1/upload', formData, {
          withCredentials: true
        });

        console.log('File uploaded successfully:', response.data);
        setReceivedFiles((prevFiles) => [...prevFiles, response.data]);
      } catch (error) {
        console.error('Error uploading file:', error);
      }
    } else {
      console.log('No file selected');
    }
  };

  return (
    <div className="App" style={{ marginTop: '100px' }}>
      <input type="file" accept=".html" onChange={handleFileChange} />
      <button onClick={handleUpload}>Upload</button>

      <div className="received-files" style={{ marginTop: '20px' }}>
        {receivedFiles.map((file) => (
            <div key={file.id} className="file-item" style={{ marginTop: '20px' }}>
              <div>Filename: {file.FileName}</div>
              <div>Status: {enumStatusText(file.Status)}</div>
              <button disabled={file.Status !== 2} onClick={() => handleDownload(file.Id)}>Download</button>
              <button onClick={() => handleDelete(file.Id)}>Delete</button>
            </div>
        ))}
      </div>
    </div>
  );
}

export default App;
