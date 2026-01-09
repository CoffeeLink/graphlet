import './App.css'
import {createBrowserRouter, RouterProvider} from "react-router-dom";
import {ROUTING} from "./components/router/router.tsx";



const router = createBrowserRouter(ROUTING);

function App() {
  

  return (
    <>
      <RouterProvider router={router}/>
    </>
  )
}

export default App
