/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { RouterProvider } from "react-router-dom";
import { router } from "./router/index.tsx";
import { ToastContainer } from "react-toastify";
import 'react-toastify/dist/ReactToastify.css';

function App() {
    return ( 
            <>
                <ToastContainer/>
            <RouterProvider router={router} /> 
        </>

    );
}

export default App;
