# VitalCheck: Health & Fitness Diagnostic

A practical, modern Windows Forms desktop application built in **C#** and **.NET 10** using **Microsoft SQL Server LocalDB** and **ADO.NET**, structured strictly according to the **Model-View-Controller (MVC)** architectural pattern.

---

## 🌟 Key Features

1. **Role-Based User Management & Authentication:**
   - **Individual Users:** Personal health tracking, automated profile creation, private health records.
   - **Fitness Instructors:** Multi-client directory, client search, client registration, and active client context switching.
   - Secure SHA-256 password hashing and parameterized SQL commands.

2. **Health & Fitness Diagnostic Assessments:**
   - Evaluates Age, Biological Sex, Height (cm & m), Weight (kg), Physical Activity Level, Daily Sleep Duration (hrs), and Daily Water Intake (L).
   - Real-time **Body Mass Index (BMI)** calculation: $\text{BMI} = \frac{\text{Weight (kg)}}{[\text{Height (m)}]^2}$.
   - **Age-Appropriate Rules:** Standard adult classifications (Underweight, Normal, Overweight, Obese) are strictly separated from pediatric/adolescent cases (< 18 years old).
   - **Rule-Based Wellness Recommendations:** Tailored advice on cardio, strength training, sleep debt recovery, and daily hydration without claiming medical diagnoses.

3. **Fitness Goals & Milestones:**
   - Create quantifiable targets (target weight, step counts, workout frequency, sleep goals).
   - Dynamic progress percentage calculation and overdue deadline alerts.
   - Update current values and mark goals as completed.

4. **Progress Tracking & Historical Analytics:**
   - Historical checkup log in DataGridView.
   - Delta comparisons: Initial vs Latest weight changes ($\pm\text{kg}$), BMI trends, and lifestyle averages.
   - Inspection of historical diagnostic feedback for any past assessment.

---

## 🏗️ MVC Architecture & Folder Structure

```text
d:\Xkl\.NET\Lab3\
├── VitalCheck.sln                     # Visual Studio Solution File
├── VitalCheck\
│   ├── VitalCheck.csproj              # Windows Forms Project Configuration
│   ├── Program.cs                     # Application Entry Point & Auto-Initialization
│   │
│   ├── Data\
│   │   ├── DatabaseHelper.cs          # ADO.NET Connection & Query Helper (Zero-config setup)
│   │   └── VitalCheckDB_Schema.sql    # Complete SQL Server Schema Script
│   │
│   ├── Models\
│   │   ├── User.cs                    # User credentials, roles, and session data
│   │   ├── ClientProfile.cs           # Physical measurements and client demographics
│   │   ├── Assessment.cs              # Assessment data, BMI logic, and wellness rules
│   │   └── FitnessGoal.cs             # Fitness targets, deadlines, and progress formulas
│   │
│   ├── Controllers\
│   │   ├── UserController.cs          # Authentication, validation, and profile linkage
│   │   ├── ClientController.cs        # Instructor client management and search
│   │   ├── AssessmentController.cs    # Diagnostic evaluation and assessment records
│   │   └── GoalController.cs          # Goal creation, updates, and completion
│   │
│   └── Views\
│       ├── LoginForm.cs               # Sign in and registration card interface
│       ├── MainDashboardForm.cs       # Master shell with modern sidebar & header
│       ├── DashboardControl.cs        # Metric summary cards and latest checkup preview
│       ├── AssessmentControl.cs       # Diagnostic input form and real-time feedback
│       ├── GoalsControl.cs            # Goals table, progress updates, and new goal form
│       ├── ProgressControl.cs         # Historical comparisons and past checkup viewer
│       └── ClientManagementControl.cs # Instructor client directory and active client picker
```

---

## 🚀 Running the Application

### Option A: Using Visual Studio
1. Open `VitalCheck.sln` in **Visual Studio 2022** (or higher).
2. Set `VitalCheck` as the Startup Project.
3. Press **F5** (or click **Start Debugging**).
4. The application will automatically check for SQL Server LocalDB, create the `VitalCheckDB` database and tables if they do not exist, and launch the **Sign In** screen.

### Option B: Using .NET CLI
```powershell
cd d:\Xkl\.NET\Lab3\VitalCheck
dotnet run
```

---

## 🗄️ Database Information

- **Database Name:** `VitalCheckDB`
- **Engine:** Microsoft SQL Server LocalDB (`(localdb)\MSSQLLocalDB`)
- **Tables:**
  - `Users` (Authentication, Role: Individual / Instructor)
  - `ClientProfiles` (Demographic and physical baseline data)
  - `Assessments` (Diagnostic checkups, BMI, sleep, water, and feedback)
  - `FitnessGoals` (Targets, current values, units, status)
- **Automatic Initialization:** Handled seamlessly by `DatabaseHelper.InitializeDatabase()` on startup.
