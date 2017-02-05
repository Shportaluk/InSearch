USE InSearch
CREATE TABLE Users
(
	Id INT IDENTITY( 1, 1 ),
	Login NVARCHAR( 40 ),
	Pass NVARCHAR( 150 ),
	FirstName NVARCHAR( 20 ),
	LastName NVARCHAR( 15 ),
	Sex NVARCHAR(10),
	Country NVARCHAR(100),
	City NVARCHAR(255),
	Phone NVARCHAR(255),
	PathImg NVARCHAR( 100 ),
	Roles NVARCHAR( 255 ),
	IsOnline bit,
	
	
	PRIMARY KEY ( Login )
)

INSERT INTO Users VALUES ( 'admin', '41806F6757B29F734568AD7B0C0EA422', 'Andrew', 'Shportaluk', 2, 'Ukraine',  'Rivne', '', 'default_man.jpg', 'Administrator', 1 )


CREATE TABLE Questions
(
	Id INT IDENTITY( 1, 1 ),
	Language NVARCHAR(10),
	Name NVARCHAR(50),
	Ask NVARCHAR(255),
	OptionsAns NVARCHAR(255),
	
	PRIMARY KEY ( Id )
)


CREATE TABLE CompletedQuestionOfUser 
(
	Id INT IDENTITY( 1, 1 ),
	Login NVARCHAR(40),
	NameQuestion NVARCHAR(50),
	Ask NVARCHAR(255),
	Ans NVARCHAR(25),
	
	
	PRIMARY KEY ( Id ),
	CONSTRAINT fk_completedQuestionOfUser_login FOREIGN KEY ( Login ) REFERENCES Users( Login )
)