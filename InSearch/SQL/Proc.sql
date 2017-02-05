USE InSearch

GO
CREATE PROC RegistrationTwo
  @login NVARCHAR(40),
  @pass NVARCHAR(150),
  @firstName NVARCHAR(20),
  @lastName NVARCHAR(20),
  @sex NVARCHAR(10),
  @coutry NVARCHAR(100),
  @city NVARCHAR(255),
  @pathImg NVARCHAR( 100 )
AS
IF ((SELECT COUNT(*) FROM Users WHERE Login = @login ) = 0)
BEGIN
	INSERT INTO Users VALUES( @login, @pass, @firstName, @lastName, @sex, @coutry, @city, null, @pathImg, 'User', 0 );
	SELECT 'OK'
END
ELSE
	SELECT 'User is already registered'
GO


GO
CREATE PROC Registration
  @login NVARCHAR(40),
  @pass NVARCHAR(150),
  @firstName NVARCHAR(20),
  @lastName NVARCHAR(20),
  @sex NVARCHAR(10)
AS
IF ((SELECT COUNT(*) FROM Users WHERE Login = @login ) = 0)
BEGIN
	INSERT INTO Users VALUES( @login, @pass, @firstName, @lastName, @sex, null, null, null, 'default_man.jpg', 'User', 0 );
	SELECT 'OK'
END
ELSE
	SELECT 'User is already registered'
GO


GO
CREATE PROC LogIn
	@login NVARCHAR(40),
	@pass NVARCHAR(150)
AS
IF( (SELECT COUNT(*) FROM Users WHERE Login = @login and Pass = @pass) = 1 )
BEGIN
	UPDATE Users SET IsOnline = 1 WHERE Login = @login
	SELECT 'OK'
END
ELSE
	SELECT 'Invalid login or password'
GO


GO
CREATE PROC LogInWithoutPass
	@login NVARCHAR(40)
AS
IF( (SELECT COUNT(*) FROM Users WHERE Login = @login) = 1 )
BEGIN
	UPDATE Users SET IsOnline = 1 WHERE Login = @login
	SELECT 'OK'
END
ELSE
	SELECT 'Invalid login or password'
GO




GO
CREATE PROC CompleteQuestion
	@login NVARCHAR(40),
	@nameQuestion NVARCHAR(255),
	@opinionsAns NVARCHAR(1024)
AS
DECLARE @delimeter NVARCHAR(1) = '|'
DECLARE @delimeter2 NVARCHAR(1) = ':'

DECLARE @pos INT = charindex(@delimeter,@opinionsAns)
DECLARE @pos2 INT
DECLARE @str NVARCHAR(255)
DECLARE @ask NVARCHAR(255)
DECLARE @ans NVARCHAR(10)
    
WHILE (@pos != 0)
BEGIN
    set @str = SUBSTRING(@opinionsAns, 1, @pos-1)
    set @pos2 = charindex(@delimeter2,@str)
    
    set @ask = SUBSTRING(@str, 1, @pos2-1)
    set @ans =  SUBSTRING(@str, @pos2+1, LEN(@str))
    
    INSERT INTO CompletedQuestionOfUser values( @login, @nameQuestion, @ask, @ans)
    
    
    set @opinionsAns = SUBSTRING(@opinionsAns, @pos+1, LEN(@opinionsAns))
    set @pos = CHARINDEX(@delimeter,@opinionsAns)
END
GO


----------- AdminPanel -----------
GO
CREATE PROC NewQuestions
	@login NVARCHAR(40),
	@nameQuestion NVARCHAR(255),
	@askAndOpinionsId INT
AS
IF( @login = 'admin' )
BEGIN
	INSERT INTO Questions VALUES( @nameQuestion, @askAndOpinionsId );
	SELECT 'OK'
END
ELSE
	SELECT 'Invalid login or password'
GO
