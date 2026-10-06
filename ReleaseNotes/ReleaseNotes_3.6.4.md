# Final Release Notes

**Version** 3.6.4 
**Date:** June 23, 2026  
**Team:** SkillTrie  
**Client:** CitoLab Foundation

---

# Summary

We are pleased to present the final product of SkillTrie, an AI-powered adaptive learning platform for education. This release delivers all Must-have, Should-have and most Could-have requirements agreed upon in our project plan, as well as many features from our second MoSCoW. It represents a fully functional learning environment for both students and researchers.

Students can complete the full learning cycle within the subject of statistics in a gamified way. Selecting topics based on prerequisites within domains, taking AI-generated tests with intuitive navigation, receiving personalized AI-generated feedback, and tracking their progress through a visual knowledge tree. This experience is gamified through the use of badges and a profile page with a customizable character.

---

# What's New in This Release

## Student Environment

Core Learning Flow

* **Topic Selection with Prerequisites** \- Students can only access topics where prerequisites are met (80% mastery threshold)  
* **Knowledge Tree Visualization V2** \- procedural generation of visual representation showing locked, available, and passed topics based on mini-metro style  
* **Next Topic Suggestions V0** \- Algorithm recommends next topic to practice after passing a test

User Experience

* **Updated User Interface** \- Clean, modern design  
* **Intuitive Flow** \- No more distractions, straight to action  
* **Artist Integration** \- Artwork from artists is incorporated throughout the application  
* **Legend** \- Information about the knowledge tree  
* **Onboarding** \- Guides the user through the core flows of the application

Test Flow

* **Navigation** \- navigation through levels, question status, progress visualisation  
* **post-test summary** \- An ordered view of the made questions and their corresponding answer

## Researcher Environment

Quality Control & Oversight

* **Separated Researcher Interface** \- Distinct environment accessible only to authenticated researchers  
* **Dashboard** \- An overview student count and reported questions  
* **Automatic item disabling V1** \- Items are disabled after a set number of reports  
* **AI-factor** \- Allows the researcher to set the factor of AI questions within a test  
* **Badge creator** \- An interface to create new badges

Data Foundation

* **Persistent Database** \- All student activity, test results, and item data stored reliably  
* **Clean Item Database** \- ShareStats content imported, validated, and properly tagged  
* **Reworked Database foundation** \- A new and improved setup of the database allows for more extendability to other subjects.

User Experience:

* **Light/Dark mode** \- Added light/dark variant based on preferred color scheme

## AI Integration

Feedback Generation

* **Speed up** \- halved generation time by better suited AI-model  
* **Improved prompts** \- Allow for the AI to generate more suitable feedback and questions

Question Generation

* **Mathematical text generation \-** The AI creates mathematical text in LaTeX, which is converted in MathML when being sent to the frontend.  
* **Bulk question generation** \- Allows the AI to generate a customizable amount of questions through the use of a single prompt.

## Adaptive Learning

Knowledge Graph

* **Prerequisite System** \- Statistical concepts organized in directed acyclic graph (DAG)  
* **Mastery Tracking** \- System tracks which topics each student has passed (≥80%)  
* **Progressive Unlocking** \- Passing a topic/domain unlocks dependent topic/domains automatically

Recommendation Algorithm

* **Automated Suggestions** \- After passing a test, system recommends next topic to practice  
* **Prerequisite-Based Selection** \- Only suggests topics where all prerequisites are met  
* **Pick-items Algorithm** \- Items within a test are picked based on student proficiency  
* **Urning Algorithm \-** Student level is determined with a complex, mathematical algorithm

## Research Infrastructure

A/B Testing

* **PostHog Integration** \- Full integration of PostHog as the A/B testing and web analytics platform, enabling researchers to run controlled experiments without developer intervention  
* **Feature Flags \-** Researchers can define and toggle feature flags through PostHog's interface, controlling which algorithm or UI variant is served to which users  
* **Live A/B Tests \-** Two A/B tests are currently active: a visual enhancement of the recommended topic suggestion (animated vs. static), and enforcing single-domain expansion on the knowledge tree (expanded one-at-a-time vs. multiple)

Web Analytics

* **Event Tracking** \- PostHog captures page views, page exits, and selected user interactions, providing insight into how the platform is used  
* **Built-in Surveys** \- PostHog's survey tool is integrated at key points in the user flow, collecting contextual feedback without requiring users to leave the application

## Gamification

Profile page

* **Badges** \- A system that automatically tracks badges and notifies on achievement  
* **Character customization** \- The options to customise the character and color palette  
* **Streaks** \- Daily streaks are tracked  
* **Theme** \- An overall coherent theme throughout the application

---

## **Technical Highlights**

Architecture

* **Frontend:** React \+ TypeScript, Tailwind CSS  
* **Backend:** ASP.NET Core (C\#)  
* **Database:** PostgreSQL with clean schema and proper indexes  
* **AI Services:** Azure GPT (client-provided API)  
* **Hosting:** Dockerized deployment on Utrecht University servers  
* **A/B Testing & Analytics:** PostHog (featue flags, web analytics, surveys, A/B experiments)  
* **UI Components:** Material UI  
* **Icon Library:** Heroicons  
* **Question rendering:** QTI Player  

Quality Assurance

* Unit test coverage on critical paths  
* Integration tests at critical workflows  
* CI/CD pipeline with automated testing  
* Code review process (1 or 2 approval(s) required for merges to dev branch)  
* Browser compatibility tested (Chrome, Firefox, Safari)  
* Mobile device testing (iOS and Android)

Security

* Role-based access control (RBAC) enforced on frontend and backend  
* Input validation and sanitization

---

## **Acknowledgments**

We would like to thank:

* **Citolab Team** (Lientje Maas, Joost Kruis, Marcel Hoekstra) for their guidance, expertise, and valuable feedback throughout development  
* **Utrecht University** for providing workspace, infrastructure, and supervisor support  
* **Previous Development Team STIFT** for the foundational codebase we built upon  
* **Artist Team** for UI/UX design contributions

---

# Technology Stack

| Component | Technology | Version |
| :---- | :---- | :---- |
| Frontend Framework | React | 19.2.7 |
| Language | TypeScript | 5.9.3 |
| CSS Framework | Tailwind CSS | 4.3.1 |
| Backend Framework | ASP.NET Core | .NET 8.0 |
| Language | C\# | C\#12 |
| Database | PostgreSQL | 18.2 |
| AI Service | Azure GPT | gpt-5.4-nano |
| Containerization | Docker | 29.5.2 |
| Analytics & A/B | PostHog | Cloud |
| UI Components | Material UI | \- |
| Question Rendering | QTI Player | 7.28.1 |
| Version Control | GitLab | \- |
| Project Management | YouTrack | \- |
