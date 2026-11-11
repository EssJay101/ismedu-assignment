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

-- Sample students are added by the API on first start (Data/SampleStudentSeeder.cs),

INSERT INTO Users (UserId, FullName, Email, PasswordHash, IsActive) VALUES
(1, 'ISMEDU Admin', 'admin@ismedu.com', '$2a$11$shgNnVVoic8wNSxXEIN.1.VmQ1jYpTvwDglETEiQArfiymDisoEhG', TRUE);