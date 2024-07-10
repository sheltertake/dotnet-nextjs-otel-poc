import React, { useState, useEffect } from 'react';

const Chat = () => {
  const [socket, setSocket] = useState<WebSocket | null>(null);
  const [inputMessage, setInputMessage] = useState('');
  const [messages, setMessages] = useState<string[]>([]);
  const [connectionStatus, setConnectionStatus] = useState('CLOSED');

  useEffect(() => {
    // Apri la connessione automaticamente al montaggio del componente
    openConnection();

    return () => {
      // Chiudi la connessione al smontaggio del componente
      if (socket) {
        socket.close();
      }
    };
  }, []);

  useEffect(() => {
    if (socket) {
      socket.onopen = () => setConnectionStatus('OPEN');
      socket.onmessage = (event) => {
        setMessages((prevMessages) => [...prevMessages, event.data]);
      };
      socket.onclose = () => setConnectionStatus('CLOSED');
      socket.onerror = () => setConnectionStatus('CLOSED');
    }
  }, [socket]);

  const sendMessage = () => {
    if (socket && inputMessage.trim() !== '') {
      socket.send(inputMessage);
      setInputMessage('');
    }
  };

  const closeConnection = () => {
    if (socket && connectionStatus === 'OPEN') {
      socket.close();
    }
  };

  const openConnection = () => {
    if (!socket || connectionStatus === 'CLOSED') {
      const newSocket = new WebSocket('ws://localhost:5262');
      setSocket(newSocket);
    }
  };
  const sendBulkMessages = () => {
    if (socket && connectionStatus === 'OPEN') {
      for (let i = 0; i < 1500; i++) {
        socket.send(i.toString());
      }
    }
  };
  return (
    <div>
      <div className="top-bar flex justify-between items-center p-4 bg-gray-200">
        <input
          type="text"
          value={inputMessage}
          onChange={(e) => setInputMessage(e.target.value)}
          placeholder="Type a message..."
          className="form-input px-4 py-2 border rounded-md flex-1 mr-4"
        />
        <button onClick={sendMessage} className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600">Send</button>
        <button onClick={closeConnection} disabled={connectionStatus !== 'OPEN'} className="ml-4 px-4 py-2 bg-red-500 text-white rounded-md hover:bg-red-600 disabled:bg-gray-400">Close</button>
        <button onClick={openConnection} disabled={connectionStatus === 'OPEN'} className="ml-4 px-4 py-2 bg-green-500 text-white rounded-md hover:bg-green-600 disabled:bg-gray-400">Open</button>
        <button onClick={sendBulkMessages} className="ml-4 px-4 py-2 bg-purple-500 text-white rounded-md hover:bg-purple-600">Send 1500 Messages</button>
      </div>
      <h2>Messages</h2>
      <ul>
        {messages.map((message, index) => (
          <li key={index}>{message}</li>
        ))}
      </ul>
    </div>
  );
};

export default Chat;