-- =========================
-- CREATE DATABASE TABLES
-- PostgreSQL Script
-- =========================

-- DROP TABLES (optional)
DROP TABLE IF EXISTS Enrollment;
DROP TABLE IF EXISTS Student;
DROP TABLE IF EXISTS Course;
DROP TABLE IF EXISTS Subject;
DROP TABLE IF EXISTS Semester;

-- =========================
-- SEMESTER
-- =========================
CREATE TABLE Semester (
    SemesterId SERIAL PRIMARY KEY,
    SemesterName VARCHAR(100) NOT NULL,
    StartDate TIMESTAMP NOT NULL,
    EndDate TIMESTAMP NOT NULL
);

-- =========================
-- COURSE
-- =========================
CREATE TABLE Course (
    CourseId SERIAL PRIMARY KEY,
    CourseName VARCHAR(100) NOT NULL,
    SemesterId INT NOT NULL,
    CONSTRAINT fk_course_semester
        FOREIGN KEY (SemesterId)
        REFERENCES Semester(SemesterId)
        ON DELETE CASCADE
);

-- =========================
-- SUBJECT
-- =========================
CREATE TABLE Subject (
    SubjectId SERIAL PRIMARY KEY,
    SubjectCode VARCHAR(20) NOT NULL UNIQUE,
    SubjectName VARCHAR(100) NOT NULL,
    Credit INT NOT NULL CHECK (Credit > 0)
);

-- =========================
-- STUDENT
-- =========================
CREATE TABLE Student (
    StudentId SERIAL PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    DateOfBirth TIMESTAMP NOT NULL
);

-- =========================
-- ENROLLMENT
-- =========================
CREATE TABLE Enrollment (
    EnrollmentId SERIAL PRIMARY KEY,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    EnrollDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL,

    CONSTRAINT fk_enrollment_student
        FOREIGN KEY (StudentId)
        REFERENCES Student(StudentId)
        ON DELETE CASCADE,

    CONSTRAINT fk_enrollment_course
        FOREIGN KEY (CourseId)
        REFERENCES Course(CourseId)
        ON DELETE CASCADE
);

-- =========================
-- INSERT SAMPLE DATA
-- =========================

-- 5 Semesters
INSERT INTO Semester (SemesterName, StartDate, EndDate)
VALUES
('Spring 2024', '2024-01-01', '2024-05-01'),
('Summer 2024', '2024-05-15', '2024-08-15'),
('Fall 2024', '2024-09-01', '2024-12-31'),
('Spring 2025', '2025-01-01', '2025-05-01'),
('Summer 2025', '2025-05-15', '2025-08-15');

-- 20 Courses
INSERT INTO Course (CourseName, SemesterId)
SELECT
    'Course ' || gs,
    ((gs - 1) % 5) + 1
FROM generate_series(1, 20) AS gs;

-- 10 Subjects
INSERT INTO Subject (SubjectCode, SubjectName, Credit)
VALUES
('PRN101', 'Programming Basics', 3),
('DBI202', 'Database Systems', 3),
('WED201', 'Web Development', 3),
('MAD101', 'Mobile Development', 3),
('SWE201', 'Software Engineering', 4),
('OSG202', 'Operating Systems', 3),
('NJS301', 'NodeJS API', 3),
('MLN111', 'Machine Learning Intro', 4),
('JPD113', 'Japanese N4', 2),
('GAM201', 'Game Development', 4);

-- 50 Students
INSERT INTO Student (FullName, Email, DateOfBirth)
SELECT
    'Student ' || gs,
    'student' || gs || '@gmail.com',
    TIMESTAMP '2000-01-01' + (gs * INTERVAL '30 day')
FROM generate_series(1, 50) AS gs;

-- 500 Enrollments
INSERT INTO Enrollment (StudentId, CourseId, EnrollDate, Status)
SELECT
    ((random() * 49)::INT + 1),
    ((random() * 19)::INT + 1),
    CURRENT_TIMESTAMP - ((random() * 365)::INT * INTERVAL '1 day'),
    CASE
        WHEN random() < 0.7 THEN 'Active'
        WHEN random() < 0.9 THEN 'Completed'
        ELSE 'Dropped'
    END
FROM generate_series(1, 500);

-- =========================
-- CHECK DATA
-- =========================

-- SELECT * FROM Semester;
-- SELECT * FROM Course;
-- SELECT * FROM Subject;
-- SELECT * FROM Student;
-- SELECT * FROM Enrollment;