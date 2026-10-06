# MVP Release Notes

**Version** 2.0.0  
**Date:** April 13, 2026  
**Team:** SkillTrie  
**Client:** CitoLab Foundation

---

# Summary

We are pleased to present the Minimum Viable Product (MVP) of SkillTrie, an AI-powered adaptive learning platform for education. This release delivers all Must-have requirements agreed upon in our project plan and represents a fully functional learning environment for both students and researchers.

Students can now complete the full learning cycle within the subject of statistics. Selecting topics based on prerequisites within domains, taking AI-generated tests, receiving personalized AI-generated feedback, and tracking their progress through a visual knowledge tree.

---

# What's New in This Release

## Student Environment

Core Learning Flow

* **Topic Selection with Prerequisites** \- Students can only access topics where prerequisites are met (80% mastery threshold)  
* **Knowledge Tree Visualization V1** \- Visual representation showing locked, available, and passed topics based on first version of mini-metro style  
* **Next Topic Suggestions V0** \- Algorithm recommends next topic to practice after passing a test

User Experience

* **Updated User Interface** \- Clean, modern design  
* **Intuitive Flow** \- No more distractions, straight to action

## Researcher Environment

Quality Control & Oversight

* **Separated Researcher Interface** \- Distinct environment accessible only to authenticated researchers

Data Foundation

* **Persistent Database** \- All student activity, test results, and item data stored reliably  
* **Clean Item Database** \- ShareStats content imported, validated, and properly tagged

User Experience:

* **Light/Dark mode** \- Added light/dark variant based on preferred color scheme

## AI Integration

Feedback Generation

* **Speed up** \- halved generation time by better suited AI-model

## Adaptive Learning

Knowledge Graph

* **Prerequisite System** \- Statistical concepts organized in directed acyclic graph (DAG)  
* **Mastery Tracking** \- System tracks which topics each student has passed (≥80%)  
* **Progressive Unlocking** \- Passing a topic/domain unlocks dependent topic/domains automatically

Recommendation Algorithm V1

* **Automated Suggestions** \- After passing a test, system recommends next topic to practice  
* **Prerequisite-Based Selection** \- Only suggests topics where all prerequisites are met

---

## **Technical Highlights**

Architecture

* **Frontend:** React \+ TypeScript, Tailwind CSS  
* **Backend:** ASP.NET Core (C\#)  
* **Database:** PostgreSQL with clean schema and proper indexes  
* **AI Services:** Azure GPT (client-provided API)  
* **Hosting:** Dockerized deployment on Utrecht University servers  

Quality Assurance

* Unit test coverage on critical paths  
* CI/CD pipeline with automated testing  
* Code review process (1 approval required for merges to dev branch)  
* Browser compatibility tested (Chrome, Firefox, Safari)  
* Mobile device testing (iOS and Android)

Security

* Role-based access control (RBAC) enforced on frontend and backend  
* Input validation and sanitization

---

## **Known Limitations & Future Work**

The following features are planned for the final 10 weeks (Should/Could priorities):

### **Planned for Next Phase**

**Researcher Analytics (Should):**

* Student metrics (active users, engagement trends)  
* Item metrics (which to review)

**Advanced AI Features (Should):**

* Simple open-ended questions (short-answer format)  
* AI grading of open questions  
* Enhanced quality controls for generated content  
* Reflection/Self-Critique

**Smarter Recommendations (Should):**

* V2 algorithm considering difficulty progression and student ability  
* Item selection, generation policy and assessment  
* Topic proficiency and improved suggestions

**Subject Extendability (Should):**

* Import custom knowledge trees (CSV)  
* Import custom question banks

**Subject Generalization (Could):**

* Support for subjects beyond statistics

**Enhanced Visualization (Could):**

* Gamified knowledge tree V2 (artist collaboration)  
* Animated unlocking of topics/domains  
* More engaging visual design

**Additional Polish (Could):**

* Initial student placement test  
* Performance optimizations  
* Accessibility improvements (WCAG 2.1 AA)

---

## **Acknowledgments**

We would like to thank:

* **Citolab Team** (Lientje Maas, Joost Kruis, Marcel Hoekstra) for their guidance, expertise, and valuable feedback throughout development  
* **Utrecht University** for providing workspace, infrastructure, and supervisor support  
* **Previous Development Team STIFT** for the foundational codebase we built upon  
* **Artist Team** for UI/UX design contributions

---

## **Demo & Access**

**Test Accounts:**  
Admin account:  
Username: [admin@cito.nl](mailto:admin@cito.nl)  
Password: Password123\!

Student account:  
Username: [student@cito.nl](mailto:student@cito.nl)  
Password: Password123\!

**Demo Flow:**

1. Log in as Student  
2. Select an available topic (e.g., "Descriptive Statistics")  
3. Take a 10-item test  
4. Review results with AI-generated feedback  
5. See knowledge tree update with newly unlocked topics  
     
1. Log in as Admin  
2. View users \+ edit them  
3. View question, edit or disable  
4. Experiment with settings

---

# Technology Stack

| Component | Technology | Version |
| :---- | :---- | :---- |
| Frontend Framework | React | 19.1.1 |
| Language | TypeScript | 5.9.3 |
| CSS Framework | Tailwind CSS | 4.1.13 |
| Backend Framework | ASP.NET Core | .NET 8.0 |
| Language | C\# | C\#12 |
| Database | PostgreSQL | 18.2 |
| AI Service | Azure GPT | gpt-5-mini |
| Containerization | Docker | 29.2.1 |
| Version Control | GitLab | \- |
| Project Management | YouTrack | \- |

