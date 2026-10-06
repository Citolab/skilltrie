/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useLevelContext } from "../../contexts/level-context.tsx";
import { useNavigate } from "react-router-dom";
import { useEffect } from "react";
import QtiPlayer from "../../components/qti-player/main-page/qti-player.tsx";
import { startQtiTourIfUnseen } from "../../components/tours/qti-tour.ts";


export function QTI() {
    const { levelResponse } = useLevelContext();
    const navigate = useNavigate();

    useEffect(() => {
        if (levelResponse === null) navigate("/home");
        void startQtiTourIfUnseen(); 
    }, [levelResponse, navigate]);

    return levelResponse && <QtiPlayer level={levelResponse} />;
}
