"use client"

import { useState, useEffect } from 'react';

// Definisce l'interfaccia per i dati del tempo
interface WeatherData {
  date: string;
  temperatureC: number;
  summary: string;
}

// Definisce il componente WeatherComponent
const WeatherComponent = () => {
  // Stato per memorizzare i dati del tempo
  const [weatherData, setWeatherData] = useState<WeatherData[]>([]);
  // Stato per gestire il caricamento
  const [loading, setLoading] = useState<boolean>(true);

  // Funzione per chiamare l'API
  const fetchWeatherData = async () => {
    setLoading(true);
    try {
      const response = await fetch('http://localhost:5131/weatherforecast');
      const data: WeatherData[] = await response.json();
      setWeatherData(data);
    } catch (error) {
      console.error('Errore nel fetch dei dati:', error);
    } finally {
      setLoading(false);
    }
  };

  // Effetto per chiamare l'API al montaggio del componente
  useEffect(() => {
    fetchWeatherData();
  }, []);

  return (
    <div>
      <h1>Weather Data</h1>
      <button onClick={fetchWeatherData}>Ricarica Dati</button>
      {loading ? (
        <p>Caricamento...</p>
      ) : (
        <ul>
          {weatherData.map((weather) => (
            <li key={weather.date}>
              <strong>{weather.date}</strong>: {weather.temperatureC}°C - {weather.summary}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default WeatherComponent;
