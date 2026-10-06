/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useNavigate } from "react-router-dom";
import Button from "../components/general-components/general-button";
import Tooltip from "../components/general-components/tooltip-mui";
import { toast } from "react-toastify";

/** This page is purely because we need a privacy statement for usability testing, this should be improved/ changed/ removed later */

export default function PrivacyStatementPage() {
    const navigate = useNavigate();
    
    return <div className="flex flex-col items-center justify-center min-h-screen pageBackground">
        <div className="flex flex-col p-5 w-full min-h-screen sm:w-6/7 md:5/7 lg:w-4/7 xl:w-3/7 2xl:w-2/7 gap-5 background paragraphText">
            {/* Page title */}
            <h1 className="titleText text-center">
                Privacy Statement
            </h1>

            {/* Content of the privacy statement */}
            <PrivacyStatementContent />

            {/* Button to return to previous page */}
            <div className="flex items-center justify-center">
                <Button
                    variant="primary"
                    onClick={() => navigate("/register")}
                >
                    Return to the register page
                </Button>
            </div>
        </div>
    </div>;
}

export function PrivacyStatementContent() {
    let email = "SkillTrie@gmail.com";

    return (
        <div className="pb-5">
            <h1 className="font-title font-bold text-text-primary text-2xl text-center">
                Privacy Statement
            </h1>
            <p>
                This statement explains what data we collect during the study, why we collect it, and how it is used.
                Please read this statement carefully before participating.
                By taking part in this study, you confirm that you have read, understood, and agreed to this privacy statement.
            </p>
            <div>
                <p>
                    What data we collect:
                </p>
                <ul className="list-disc pl-6">
                    <li>Any responses, comments, ratings, or other feedback you give during this survey.</li>
                    <li>Any data gathered during the session when using the webpage.</li>
                </ul>
            </div>
            <p>
                Your participant number and associated feedback will be retained for a maximum of 2 years following the completion of the study, after which they will be securely deleted.
                Aggregated and anonymised findings may be retained indefinitely.
                We do not sell, rent, or share your data with third parties.
                Data may be shared internally for development purposes.
            </p>
            <p>
                If you have any questions or comments about how your data is handled, please contact us using {" "}
                <Tooltip
                    title="Copy to clipboard"
                    disableHoverListener={false}
                >
                    <button
                        title="Copy to clipboard"
                        onClick={() => {navigator.clipboard.writeText(email);
                                        toast.success("Copied to clipboard");
                        }}
                        className="underline hover:text-secondary"
                    >
                        {email}
                    </button>
                </Tooltip>.
            </p>
        </div>
    );
}
