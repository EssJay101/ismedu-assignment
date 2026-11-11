USE ismedu_db;

DELIMITER $$

-- USERS

DROP PROCEDURE IF EXISTS sp_User_GetByEmail $$
CREATE PROCEDURE sp_User_GetByEmail(IN p_Email VARCHAR(254))
BEGIN
    SELECT UserId, FullName, Email, PasswordHash, IsActive, CreatedAt
    FROM Users
    WHERE Email = p_Email
    LIMIT 1;
END $$

-- COUNTRIES

DROP PROCEDURE IF EXISTS sp_Country_GetPaged $$
CREATE PROCEDURE sp_Country_GetPaged(
    IN p_Search   VARCHAR(100),
    IN p_IsActive BOOLEAN,
    IN p_Page     INT,
    IN p_PageSize INT)
BEGIN
    DECLARE v_Offset  INT DEFAULT (p_Page - 1) * p_PageSize;
    DECLARE v_Pattern VARCHAR(102) DEFAULT CONCAT('%', p_Search, '%');

    SELECT COUNT(*) AS TotalCount
    FROM Countries
    WHERE (p_Search IS NULL OR CountryName LIKE v_Pattern OR CountryCode LIKE v_Pattern)
      AND (p_IsActive IS NULL OR IsActive = p_IsActive);

    SELECT CountryId, CountryName, CountryCode, IsActive
    FROM Countries
    WHERE (p_Search IS NULL OR CountryName LIKE v_Pattern OR CountryCode LIKE v_Pattern)
      AND (p_IsActive IS NULL OR IsActive = p_IsActive)
    ORDER BY CountryId
    LIMIT p_PageSize OFFSET v_Offset;
END $$

DROP PROCEDURE IF EXISTS sp_Country_GetById $$
CREATE PROCEDURE sp_Country_GetById(IN p_CountryId INT)
BEGIN
    SELECT CountryId, CountryName, CountryCode, IsActive
    FROM Countries
    WHERE CountryId = p_CountryId;
END $$

DROP PROCEDURE IF EXISTS sp_Country_GetActive $$
CREATE PROCEDURE sp_Country_GetActive()
BEGIN
    SELECT CountryId, CountryName
    FROM Countries
    WHERE IsActive = TRUE
    ORDER BY CountryName;
END $$

DROP PROCEDURE IF EXISTS sp_Country_NameExists $$
CREATE PROCEDURE sp_Country_NameExists(IN p_CountryName VARCHAR(100), IN p_ExcludeId INT)
BEGIN
    SELECT EXISTS(
        SELECT 1 FROM Countries
        WHERE CountryName = p_CountryName
          AND (p_ExcludeId IS NULL OR CountryId <> p_ExcludeId)
    ) AS NameExists;
END $$

DROP PROCEDURE IF EXISTS sp_Country_CodeExists $$
CREATE PROCEDURE sp_Country_CodeExists(IN p_CountryCode VARCHAR(3), IN p_ExcludeId INT)
BEGIN
    SELECT EXISTS(
        SELECT 1 FROM Countries
        WHERE CountryCode = p_CountryCode
          AND (p_ExcludeId IS NULL OR CountryId <> p_ExcludeId)
    ) AS CodeExists;
END $$

DROP PROCEDURE IF EXISTS sp_Country_Insert $$
CREATE PROCEDURE sp_Country_Insert(
    IN p_CountryName VARCHAR(100),
    IN p_CountryCode VARCHAR(3),
    IN p_IsActive    BOOLEAN)
BEGIN
    INSERT INTO Countries (CountryName, CountryCode, IsActive)
    VALUES (p_CountryName, p_CountryCode, p_IsActive);

    SELECT LAST_INSERT_ID() AS NewId;
END $$

DROP PROCEDURE IF EXISTS sp_Country_Update $$
CREATE PROCEDURE sp_Country_Update(
    IN p_CountryId   INT,
    IN p_CountryName VARCHAR(100),
    IN p_CountryCode VARCHAR(3),
    IN p_IsActive    BOOLEAN)
BEGIN
    UPDATE Countries
    SET CountryName = p_CountryName,
        CountryCode = p_CountryCode,
        IsActive    = p_IsActive
    WHERE CountryId = p_CountryId;

    SELECT ROW_COUNT() AS RowsAffected;
END $$

DROP PROCEDURE IF EXISTS sp_Country_Delete $$
CREATE PROCEDURE sp_Country_Delete(IN p_CountryId INT)
BEGIN
    DELETE FROM Countries WHERE CountryId = p_CountryId;

    SELECT ROW_COUNT() AS RowsAffected;
END $$

DROP PROCEDURE IF EXISTS sp_Country_GetUsage $$
CREATE PROCEDURE sp_Country_GetUsage(IN p_CountryId INT)
BEGIN
    SELECT
        (SELECT COUNT(*) FROM States   WHERE CountryId = p_CountryId) AS StateCount,
        (SELECT COUNT(*) FROM Students WHERE CountryId = p_CountryId) AS StudentCount;
END $$

-- STATES

DROP PROCEDURE IF EXISTS sp_State_GetPaged $$
CREATE PROCEDURE sp_State_GetPaged(
    IN p_Search    VARCHAR(100),
    IN p_CountryId INT,
    IN p_IsActive  BOOLEAN,
    IN p_Page      INT,
    IN p_PageSize  INT)
BEGIN
    DECLARE v_Offset  INT DEFAULT (p_Page - 1) * p_PageSize;
    DECLARE v_Pattern VARCHAR(102) DEFAULT CONCAT('%', p_Search, '%');

    SELECT COUNT(*) AS TotalCount
    FROM States s
    INNER JOIN Countries c ON c.CountryId = s.CountryId
    WHERE (p_Search IS NULL OR s.StateName LIKE v_Pattern OR c.CountryName LIKE v_Pattern)
      AND (p_CountryId IS NULL OR s.CountryId = p_CountryId)
      AND (p_IsActive IS NULL OR s.IsActive = p_IsActive);

    SELECT s.StateId, s.StateName, s.CountryId, s.IsActive, c.CountryName
    FROM States s
    INNER JOIN Countries c ON c.CountryId = s.CountryId
    WHERE (p_Search IS NULL OR s.StateName LIKE v_Pattern OR c.CountryName LIKE v_Pattern)
      AND (p_CountryId IS NULL OR s.CountryId = p_CountryId)
      AND (p_IsActive IS NULL OR s.IsActive = p_IsActive)
    ORDER BY s.StateId
    LIMIT p_PageSize OFFSET v_Offset;
END $$

DROP PROCEDURE IF EXISTS sp_State_GetById $$
CREATE PROCEDURE sp_State_GetById(IN p_StateId INT)
BEGIN
    SELECT s.StateId, s.StateName, s.CountryId, s.IsActive, c.CountryName
    FROM States s
    INNER JOIN Countries c ON c.CountryId = s.CountryId
    WHERE s.StateId = p_StateId;
END $$

DROP PROCEDURE IF EXISTS sp_State_GetActiveByCountry $$
CREATE PROCEDURE sp_State_GetActiveByCountry(IN p_CountryId INT)
BEGIN
    SELECT StateId, StateName
    FROM States
    WHERE CountryId = p_CountryId AND IsActive = TRUE
    ORDER BY StateName;
END $$

DROP PROCEDURE IF EXISTS sp_State_NameExists $$
CREATE PROCEDURE sp_State_NameExists(
    IN p_StateName VARCHAR(100),
    IN p_CountryId INT,
    IN p_ExcludeId INT)
BEGIN
    SELECT EXISTS(
        SELECT 1 FROM States
        WHERE StateName = p_StateName
          AND CountryId = p_CountryId
          AND (p_ExcludeId IS NULL OR StateId <> p_ExcludeId)
    ) AS NameExists;
END $$

DROP PROCEDURE IF EXISTS sp_State_Insert $$
CREATE PROCEDURE sp_State_Insert(
    IN p_StateName VARCHAR(100),
    IN p_CountryId INT,
    IN p_IsActive  BOOLEAN)
BEGIN
    INSERT INTO States (StateName, CountryId, IsActive)
    VALUES (p_StateName, p_CountryId, p_IsActive);

    SELECT LAST_INSERT_ID() AS NewId;
END $$

DROP PROCEDURE IF EXISTS sp_State_Update $$
CREATE PROCEDURE sp_State_Update(
    IN p_StateId   INT,
    IN p_StateName VARCHAR(100),
    IN p_CountryId INT,
    IN p_IsActive  BOOLEAN)
BEGIN
    UPDATE States
    SET StateName = p_StateName,
        CountryId = p_CountryId,
        IsActive  = p_IsActive
    WHERE StateId = p_StateId;

    SELECT ROW_COUNT() AS RowsAffected;
END $$

DROP PROCEDURE IF EXISTS sp_State_Delete $$
CREATE PROCEDURE sp_State_Delete(IN p_StateId INT)
BEGIN
    DELETE FROM States WHERE StateId = p_StateId;

    SELECT ROW_COUNT() AS RowsAffected;
END $$

DROP PROCEDURE IF EXISTS sp_State_GetUsage $$
CREATE PROCEDURE sp_State_GetUsage(IN p_StateId INT)
BEGIN
    SELECT
        (SELECT COUNT(*) FROM Cities   WHERE StateId = p_StateId) AS CityCount,
        (SELECT COUNT(*) FROM Students WHERE StateId = p_StateId) AS StudentCount;
END $$

-- CITIES

DROP PROCEDURE IF EXISTS sp_City_GetPaged $$
CREATE PROCEDURE sp_City_GetPaged(
    IN p_Search    VARCHAR(100),
    IN p_CountryId INT,
    IN p_StateId   INT,
    IN p_IsActive  BOOLEAN,
    IN p_Page      INT,
    IN p_PageSize  INT)
BEGIN
    DECLARE v_Offset  INT DEFAULT (p_Page - 1) * p_PageSize;
    DECLARE v_Pattern VARCHAR(102) DEFAULT CONCAT('%', p_Search, '%');

    SELECT COUNT(*) AS TotalCount
    FROM Cities ci
    INNER JOIN States s    ON s.StateId   = ci.StateId
    INNER JOIN Countries c ON c.CountryId = s.CountryId
    WHERE (p_Search IS NULL
           OR ci.CityName   LIKE v_Pattern
           OR s.StateName   LIKE v_Pattern
           OR c.CountryName LIKE v_Pattern)
      AND (p_StateId   IS NULL OR ci.StateId  = p_StateId)
      AND (p_CountryId IS NULL OR s.CountryId = p_CountryId)
      AND (p_IsActive  IS NULL OR ci.IsActive = p_IsActive);

    SELECT ci.CityId, ci.CityName, ci.StateId, ci.IsActive,
           s.StateName, s.CountryId, c.CountryName
    FROM Cities ci
    INNER JOIN States s    ON s.StateId   = ci.StateId
    INNER JOIN Countries c ON c.CountryId = s.CountryId
    WHERE (p_Search IS NULL
           OR ci.CityName   LIKE v_Pattern
           OR s.StateName   LIKE v_Pattern
           OR c.CountryName LIKE v_Pattern)
      AND (p_StateId   IS NULL OR ci.StateId  = p_StateId)
      AND (p_CountryId IS NULL OR s.CountryId = p_CountryId)
      AND (p_IsActive  IS NULL OR ci.IsActive = p_IsActive)
    ORDER BY ci.CityId
    LIMIT p_PageSize OFFSET v_Offset;
END $$

DROP PROCEDURE IF EXISTS sp_City_GetById $$
CREATE PROCEDURE sp_City_GetById(IN p_CityId INT)
BEGIN
    SELECT ci.CityId, ci.CityName, ci.StateId, ci.IsActive,
           s.StateName, s.CountryId, c.CountryName
    FROM Cities ci
    INNER JOIN States s    ON s.StateId   = ci.StateId
    INNER JOIN Countries c ON c.CountryId = s.CountryId
    WHERE ci.CityId = p_CityId;
END $$

DROP PROCEDURE IF EXISTS sp_City_GetActiveByState $$
CREATE PROCEDURE sp_City_GetActiveByState(IN p_StateId INT)
BEGIN
    SELECT CityId, CityName
    FROM Cities
    WHERE StateId = p_StateId AND IsActive = TRUE
    ORDER BY CityName;
END $$

DROP PROCEDURE IF EXISTS sp_City_NameExists $$
CREATE PROCEDURE sp_City_NameExists(
    IN p_CityName  VARCHAR(100),
    IN p_StateId   INT,
    IN p_ExcludeId INT)
BEGIN
    SELECT EXISTS(
        SELECT 1 FROM Cities
        WHERE CityName = p_CityName
          AND StateId = p_StateId
          AND (p_ExcludeId IS NULL OR CityId <> p_ExcludeId)
    ) AS NameExists;
END $$

DROP PROCEDURE IF EXISTS sp_City_Insert $$
CREATE PROCEDURE sp_City_Insert(
    IN p_CityName VARCHAR(100),
    IN p_StateId  INT,
    IN p_IsActive BOOLEAN)
BEGIN
    INSERT INTO Cities (CityName, StateId, IsActive)
    VALUES (p_CityName, p_StateId, p_IsActive);

    SELECT LAST_INSERT_ID() AS NewId;
END $$

DROP PROCEDURE IF EXISTS sp_City_Update $$
CREATE PROCEDURE sp_City_Update(
    IN p_CityId   INT,
    IN p_CityName VARCHAR(100),
    IN p_StateId  INT,
    IN p_IsActive BOOLEAN)
BEGIN
    UPDATE Cities
    SET CityName = p_CityName,
        StateId  = p_StateId,
        IsActive = p_IsActive
    WHERE CityId = p_CityId;

    SELECT ROW_COUNT() AS RowsAffected;
END $$

DROP PROCEDURE IF EXISTS sp_City_Delete $$
CREATE PROCEDURE sp_City_Delete(IN p_CityId INT)
BEGIN
    DELETE FROM Cities WHERE CityId = p_CityId;

    SELECT ROW_COUNT() AS RowsAffected;
END $$

DROP PROCEDURE IF EXISTS sp_City_GetStudentCount $$
CREATE PROCEDURE sp_City_GetStudentCount(IN p_CityId INT)
BEGIN
    SELECT COUNT(*) AS StudentCount
    FROM Students
    WHERE CityId = p_CityId;
END $$

-- STUDENTS
DROP PROCEDURE IF EXISTS sp_Student_GetPaged $$
CREATE PROCEDURE sp_Student_GetPaged(
    IN p_Search     VARCHAR(100),
    IN p_SearchHash CHAR(64),
    IN p_CountryId  INT,
    IN p_StateId    INT,
    IN p_CityId     INT,
    IN p_Page       INT,
    IN p_PageSize   INT)
BEGIN
    DECLARE v_Offset  INT DEFAULT (p_Page - 1) * p_PageSize;
    DECLARE v_Pattern VARCHAR(102) DEFAULT CONCAT('%', p_Search, '%');

    -- Names: partial match. Email/mobile: exact match on the fingerprint.
    SELECT COUNT(*) AS TotalCount
    FROM Students st
    WHERE (p_Search IS NULL
           OR st.FirstName LIKE v_Pattern
           OR st.LastName  LIKE v_Pattern
           OR CONCAT(st.FirstName, ' ', st.LastName) LIKE v_Pattern
           OR st.EmailHash  = p_SearchHash
           OR st.MobileHash = p_SearchHash)
      AND (p_CountryId IS NULL OR st.CountryId = p_CountryId)
      AND (p_StateId   IS NULL OR st.StateId   = p_StateId)
      AND (p_CityId    IS NULL OR st.CityId    = p_CityId);

    SELECT st.StudentId, st.FirstName, st.LastName, st.DateOfBirth,
           st.Email, st.Mobile, st.CountryId, st.StateId, st.CityId,
           st.Address, st.CreatedAt, st.UpdatedAt,
           c.CountryName, s.StateName, ci.CityName
    FROM Students st
    INNER JOIN Countries c ON c.CountryId = st.CountryId
    INNER JOIN States s    ON s.StateId   = st.StateId
    INNER JOIN Cities ci   ON ci.CityId   = st.CityId
    WHERE (p_Search IS NULL
           OR st.FirstName LIKE v_Pattern
           OR st.LastName  LIKE v_Pattern
           OR CONCAT(st.FirstName, ' ', st.LastName) LIKE v_Pattern
           OR st.EmailHash  = p_SearchHash
           OR st.MobileHash = p_SearchHash)
      AND (p_CountryId IS NULL OR st.CountryId = p_CountryId)
      AND (p_StateId   IS NULL OR st.StateId   = p_StateId)
      AND (p_CityId    IS NULL OR st.CityId    = p_CityId)
    ORDER BY st.StudentId
    LIMIT p_PageSize OFFSET v_Offset;
END $$

DROP PROCEDURE IF EXISTS sp_Student_GetById $$
CREATE PROCEDURE sp_Student_GetById(IN p_StudentId INT)
BEGIN
    SELECT st.StudentId, st.FirstName, st.LastName, st.DateOfBirth,
           st.Email, st.Mobile, st.CountryId, st.StateId, st.CityId,
           st.Address, st.CreatedAt, st.UpdatedAt,
           c.CountryName, s.StateName, ci.CityName
    FROM Students st
    INNER JOIN Countries c ON c.CountryId = st.CountryId
    INNER JOIN States s    ON s.StateId   = st.StateId
    INNER JOIN Cities ci   ON ci.CityId   = st.CityId
    WHERE st.StudentId = p_StudentId;
END $$

DROP PROCEDURE IF EXISTS sp_Student_EmailExists $$
CREATE PROCEDURE sp_Student_EmailExists(IN p_EmailHash CHAR(64), IN p_ExcludeId INT)
BEGIN
    SELECT EXISTS(
        SELECT 1 FROM Students
        WHERE EmailHash = p_EmailHash
          AND (p_ExcludeId IS NULL OR StudentId <> p_ExcludeId)
    ) AS EmailExists;
END $$

DROP PROCEDURE IF EXISTS sp_Student_MobileExists $$
CREATE PROCEDURE sp_Student_MobileExists(IN p_MobileHash CHAR(64), IN p_ExcludeId INT)
BEGIN
    SELECT EXISTS(
        SELECT 1 FROM Students
        WHERE MobileHash = p_MobileHash
          AND (p_ExcludeId IS NULL OR StudentId <> p_ExcludeId)
    ) AS MobileExists;
END $$

DROP PROCEDURE IF EXISTS sp_Student_Insert $$
CREATE PROCEDURE sp_Student_Insert(
    IN p_FirstName   VARCHAR(50),
    IN p_LastName    VARCHAR(50),
    IN p_DateOfBirth DATE,
    IN p_Email       VARCHAR(1400),
    IN p_EmailHash   CHAR(64),
    IN p_Mobile      VARCHAR(64),
    IN p_MobileHash  CHAR(64),
    IN p_CountryId   INT,
    IN p_StateId     INT,
    IN p_CityId      INT,
    IN p_Address     VARCHAR(250))
BEGIN
    INSERT INTO Students
        (FirstName, LastName, DateOfBirth, Email, EmailHash, Mobile, MobileHash,
         CountryId, StateId, CityId, Address)
    VALUES
        (p_FirstName, p_LastName, p_DateOfBirth, p_Email, p_EmailHash, p_Mobile, p_MobileHash,
         p_CountryId, p_StateId, p_CityId, p_Address);

    SELECT LAST_INSERT_ID() AS NewId;
END $$

DROP PROCEDURE IF EXISTS sp_Student_Update $$
CREATE PROCEDURE sp_Student_Update(
    IN p_StudentId   INT,
    IN p_FirstName   VARCHAR(50),
    IN p_LastName    VARCHAR(50),
    IN p_DateOfBirth DATE,
    IN p_Email       VARCHAR(1400),
    IN p_EmailHash   CHAR(64),
    IN p_Mobile      VARCHAR(64),
    IN p_MobileHash  CHAR(64),
    IN p_CountryId   INT,
    IN p_StateId     INT,
    IN p_CityId      INT,
    IN p_Address     VARCHAR(250))
BEGIN
    UPDATE Students
    SET FirstName   = p_FirstName,
        LastName    = p_LastName,
        DateOfBirth = p_DateOfBirth,
        Email       = p_Email,
        EmailHash   = p_EmailHash,
        Mobile      = p_Mobile,
        MobileHash  = p_MobileHash,
        CountryId   = p_CountryId,
        StateId     = p_StateId,
        CityId      = p_CityId,
        Address     = p_Address
    WHERE StudentId = p_StudentId;

    SELECT ROW_COUNT() AS RowsAffected;
END $$

DROP PROCEDURE IF EXISTS sp_Student_Delete $$
CREATE PROCEDURE sp_Student_Delete(IN p_StudentId INT)
BEGIN
    DELETE FROM Students WHERE StudentId = p_StudentId;

    SELECT ROW_COUNT() AS RowsAffected;
END $$

DELIMITER ;