CREATE DATABASE CalendarApp_DB;
GO

USE CalendarApp_DB;
GO

CREATE TABLE Users (
    UserId NVARCHAR(50) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100)
);

CREATE TABLE Appointments (
    Id NVARCHAR(50) PRIMARY KEY,
    UserId NVARCHAR(50) NOT NULL REFERENCES Users(UserId),
    Name NVARCHAR(100) NOT NULL,
    Location NVARCHAR(200),
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL
);

CREATE TABLE Reminders (
    Id INT IDENTITY PRIMARY KEY,
    AppointmentId NVARCHAR(50) NOT NULL REFERENCES Appointments(Id),
    AlertTime DATETIME NOT NULL,
    Type NVARCHAR(50)
);

CREATE TABLE GroupMeetings (
    Id NVARCHAR(50) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Location NVARCHAR(200),
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    MeetingCode NVARCHAR(50)
);

CREATE TABLE GroupMeetingParticipants (
    MeetingId NVARCHAR(50) NOT NULL REFERENCES GroupMeetings(Id),
    UserId NVARCHAR(50) NOT NULL REFERENCES Users(UserId),
    PRIMARY KEY (MeetingId, UserId)
);

INSERT INTO Users VALUES ('u001', 'Nguyen Van A', 'a@email.com');
INSERT INTO GroupMeetings VALUES ('gm001', 'Team Standup', 'Room 101',
    '2026-05-06 09:00', '2026-05-06 09:30', 'STAND001');
