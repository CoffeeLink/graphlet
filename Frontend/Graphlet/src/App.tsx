//import { useState } from 'react'
import { useState } from 'react';
import './App.css'
import Login from './components/loginregister/login';
import Register from './components/loginregister/register';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';


function App() {
  

  return (
    <>
      <Login/>
      <BrowserRouter>
        <Routes>
          <Route path='/login' element={<Login/>}/>
        </Routes>
      </BrowserRouter>
    </>
  )
}

export default App
