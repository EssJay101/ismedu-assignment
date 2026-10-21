USE ismedu_db;

INSERT INTO
    Countries (CountryId, CountryName, CountryCode, IsActive)
VALUES
    (1, 'India', 'IN', TRUE),
    (2, 'Kazakhstan', 'KZ', TRUE),
    (3, 'United States', 'US', TRUE);

INSERT INTO
    States (StateId, StateName, CountryId, IsActive)
VALUES
    (1, 'Maharashtra', 1, TRUE),
    (2, 'Karnataka', 1, TRUE),
    (3, 'Delhi', 1, TRUE),
    (4, 'Akmola Region', 2, TRUE),
    (5, 'Karaganda Region', 2, TRUE);

INSERT INTO
    Cities (CityId, CityName, StateId, IsActive)
VALUES
    (1, 'Mumbai', 1, TRUE),
    (2, 'Pune', 1, TRUE),
    (3, 'Bengaluru', 2, TRUE),
    (4, 'Mysuru', 2, TRUE),
    (5, 'New Delhi', 3, TRUE),
    (6, 'Kokshetau', 4, TRUE),
    (7, 'Karaganda', 5, TRUE),
    (8, 'Temirtau', 5, TRUE);

INSERT INTO
    Students (
        StudentId,
        FirstName,
        LastName,
        DateOfBirth,
        Email,
        Mobile,
        CountryId,
        StateId,
        CityId,
        Address
    )
VALUES
    (
        1,
        'Rahul',
        'Sharma',
        '2003-05-14',
        'rahul.sharma@example.com',
        '9876543210',
        1,
        1,
        1,
        '12 Marine Drive, Colaba, Mumbai'
    ),
    (
        2,
        'Priya',
        'Nair',
        '2004-11-02',
        'priya.nair@example.com',
        '9123456780',
        1,
        2,
        3,
        '45 MG Road, Indiranagar, Bengaluru'
    ),
    (
        3,
        'Aditya',
        'D''Souza',
        '2002-08-23',
        'aditya.dsouza@example.com',
        '8765432109',
        1,
        1,
        2,
        '7 FC Road, Shivajinagar, Pune'
    );