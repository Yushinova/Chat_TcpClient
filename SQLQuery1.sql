create table Users
(
Id_user int not null primary key identity(1,1),
Name varchar(50) not null,
Login varchar(50) not null unique,
Password varchar(50) not null,
isActual int not null DEFAULT (1)
)
go
create table Messages
(
Id_message int not null primary key identity(1,1),
Text varchar(2000) not null,
Attachment_path varchar(100) not null unique,
Id_to_user int foreign key references Users(Id_user),
Id_from_user int not null,
Time_send datetime not null
)