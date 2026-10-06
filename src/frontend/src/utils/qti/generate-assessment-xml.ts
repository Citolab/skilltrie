/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { XMLBuilder } from "fast-xml-parser";

// boilerplate structure for an assessment.xml
import boilerplate from "./assessmentTemplate.json";

function GenerateAssessmentFromTest(...items: string[]) {
    const builder = new XMLBuilder({
        ignoreAttributes: false,
        suppressEmptyNode: true,
        format: true,
    });

    //@ts-expect-error
    boilerplate["qti-assessment-test"]["qti-test-part"]["qti-assessment-section"][
        "qti-assessment-item-ref"
    ] = items.map((item, i) => {
        return {
            "@_identifier": `Q${i + 1}`,
            "@_href": item,
        };
    });

    // convert json to XML

    return builder.build(boilerplate);
}

export default GenerateAssessmentFromTest;
