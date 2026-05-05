CREATE DATABASE IF NOT EXISTS CalendarApp_DB;
USE CalendarApp_DB;

CREATE TABLE Users (
    UserId VARCHAR(50) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(100)
);

CREATE TABLE Appointments (
    Id VARCHAR(50) PRIMARY KEY,
    UserId VARCHAR(50) NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Location VARCHAR(200),
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE TABLE Reminders (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AppointmentId VARCHAR(50) NOT NULL,
    AlertTime DATETIME NOT NULL,
    Type VARCHAR(50),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE
);

CREATE TABLE GroupMeetings (
    Id VARCHAR(50) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Location VARCHAR(200),
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    MeetingCode VARCHAR(50)
);

CREATE TABLE GroupMeetingParticipants (
    MeetingId VARCHAR(50) NOT NULL,
    UserId VARCHAR(50) NOT NULL,
    PRIMARY KEY (MeetingId, UserId),
    FOREIGN KEY (MeetingId) REFERENCES GroupMeetings(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

INSERT INTO Users (UserId, Name, Email) VALUES ('u001', 'Nguyen Van A', 'a@email.com');
INSERT INTO GroupMeetings (Id, Name, Location, StartTime, EndTime, MeetingCode) 
VALUES ('gm001', 'Team Standup', 'Room 101', '2026-05-06 09:00:00', '2026-05-06 09:30:00', 'STAND001');
