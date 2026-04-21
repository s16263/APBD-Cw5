CREATE PROCEDURE PromoteStudents
    @Studies VARCHAR(100),
    @Semester INT
AS
BEGIN
    BEGIN TRANSACTION;

    DECLARE @IdStudy INT;
    DECLARE @OldEnrollmentId INT;
    DECLARE @NewEnrollmentId INT;

    SELECT @IdStudy = IdStudy
    FROM Studies
    WHERE Name = @Studies;

    IF @IdStudy IS NULL
    BEGIN
        ROLLBACK;
        RAISERROR('No such studies', 16, 1);
        RETURN;
    END

    SELECT @OldEnrollmentId = IdEnrollment
    FROM Enrollment
    WHERE IdStudy = @IdStudy AND Semester = @Semester;

    IF @OldEnrollmentId IS NULL
    BEGIN
        ROLLBACK;
        RAISERROR('No such enrollment', 16, 1);
        RETURN;
    END

    SELECT @NewEnrollmentId = IdEnrollment
    FROM Enrollment
    WHERE IdStudy = @IdStudy AND Semester = @Semester + 1;

    IF @NewEnrollmentId IS NULL
    BEGIN
        SELECT @NewEnrollmentId = ISNULL(MAX(IdEnrollment), 0) + 1
        FROM Enrollment;

        INSERT INTO Enrollment(IdEnrollment, Semester, IdStudy, StartDate)
        VALUES(@NewEnrollmentId, @Semester + 1, @IdStudy, GETDATE());
    END

    UPDATE Student
    SET IdEnrollment = @NewEnrollmentId
    WHERE IdEnrollment = @OldEnrollmentId;

    SELECT IdEnrollment, Semester, IdStudy, StartDate
    FROM Enrollment
    WHERE IdEnrollment = @NewEnrollmentId;

    COMMIT;
END