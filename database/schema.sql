CREATE DATABASE ismedu_db CHARACTER
SET
    utf8mb4 COLLATE utf8mb4_0900_ai_ci;

USE ismedu_db;

CREATE TABLE
    Users (
        UserId INT NOT NULL AUTO_INCREMENT,
        FullName VARCHAR(100) NOT NULL,
        Email VARCHAR(254) NOT NULL,
        PasswordHash VARCHAR(255) NOT NULL,
        IsActive BOOLEAN NOT NULL DEFAULT TRUE,
        CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT PK_Users PRIMARY KEY (UserId),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );

CREATE TABLE
    Countries (
        CountryId INT NOT NULL AUTO_INCREMENT,
        CountryName VARCHAR(100) NOT NULL,
        CountryCode VARCHAR(3) NULL,
        IsActive BOOLEAN NOT NULL DEFAULT TRUE,
        CONSTRAINT PK_Countries PRIMARY KEY (CountryId),
        CONSTRAINT UQ_Countries_Name UNIQUE (CountryName),
        CONSTRAINT UQ_Countries_Code UNIQUE (CountryCode)
    );

CREATE TABLE
    States (
        StateId INT NOT NULL AUTO_INCREMENT,
        StateName VARCHAR(100) NOT NULL,
        CountryId INT NOT NULL,
        IsActive BOOLEAN NOT NULL DEFAULT TRUE,
        CONSTRAINT PK_States PRIMARY KEY (StateId),
        CONSTRAINT UQ_States_Country_Name UNIQUE (CountryId, StateName),
        CONSTRAINT FK_States_Countries FOREIGN KEY (CountryId) REFERENCES Countries (CountryId) ON DELETE RESTRICT ON UPDATE RESTRICT
    );

CREATE TABLE
    Cities (
        CityId INT NOT NULL AUTO_INCREMENT,
        CityName VARCHAR(100) NOT NULL,
        StateId INT NOT NULL,
        IsActive BOOLEAN NOT NULL DEFAULT TRUE,
        CONSTRAINT PK_Cities PRIMARY KEY (CityId),
        CONSTRAINT UQ_Cities_State_Name UNIQUE (StateId, CityName),
        CONSTRAINT FK_Cities_States FOREIGN KEY (StateId) REFERENCES States (StateId) ON DELETE RESTRICT ON UPDATE RESTRICT
    );

CREATE TABLE
    Students (
        StudentId INT NOT NULL AUTO_INCREMENT,
        FirstName VARCHAR(50) NOT NULL,
        LastName VARCHAR(50) NOT NULL,
        DateOfBirth DATE NOT NULL,
        Email VARCHAR(254) NOT NULL,
        Mobile CHAR(10) NOT NULL,
        CountryId INT NOT NULL,
        StateId INT NOT NULL,
        CityId INT NOT NULL,
        Address VARCHAR(250) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
        CONSTRAINT PK_Students PRIMARY KEY (StudentId),
        CONSTRAINT UQ_Students_Email UNIQUE (Email),
        CONSTRAINT UQ_Students_Mobile UNIQUE (Mobile),
        CONSTRAINT CK_Students_Mobile CHECK (Mobile REGEXP '^[6-9][0-9]{9}$'),
        CONSTRAINT FK_Students_Countries FOREIGN KEY (CountryId) REFERENCES Countries (CountryId) ON DELETE RESTRICT,
        CONSTRAINT FK_Students_States FOREIGN KEY (StateId) REFERENCES States (StateId) ON DELETE RESTRICT,
        CONSTRAINT FK_Students_Cities FOREIGN KEY (CityId) REFERENCES Cities (CityId) ON DELETE RESTRICT
    );