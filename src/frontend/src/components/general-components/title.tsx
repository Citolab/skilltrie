/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import VersionBadge from "./version-badge.tsx";
import { useNavigate } from "react-router-dom";

export const Title = () => {
    const navigate = useNavigate();
    return (
        <nav
            className="absolute top-0 left-0 px-6 py-4 text-2xl font-bold text-primary-dark cursor-pointer"
            onClick={() => navigate("/")}
        >
            SKILLTRIE <VersionBadge></VersionBadge>
        </nav>
    );
};
