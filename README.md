## Name
test-backend

## Scope
A minimal order management system with:
- Microservices
- REST API
- EntityFramework for SQL Server database
- Serilog for logging
- Kafka
- Refit
- Polly
- XUnit Test

## Description
- VisualStudio2022
- Framework net6.0
- Each microservice manages its own database
- Communication between microservices occurs via Kafka messageBus
- The WebApi project exposes Api Rest to perform CRUD operations on entities.
- The WebApi project sends sends the received request to the specific microservice, each with a specific topic.<br>
 The microservice performs the operation and sends a response message to the Producer (WebApi) which listens on the Controller to allow the caller to be answered

## Dependencies Projects
Test.Backend.Abstractions<br>

Test.Backend.Database -> Test.Backend.Abstractions<br>
Test.Backend.HttpClient -> Test.Backend.Abstractions<br>
Test.Backend.Kafka -> Test.Backend.Abstractions<br>

Test.Backend.Dependencies -> Test.Backend.Database<br>
Test.Backend.Dependencies -> Test.Backend.HttpClient<br>
Test.Backend.Dependencies -> Test.Backend.Kafka<br>

Test.Backend.WebApi -> Test.Backend.Dependencies<br>

Test.Backend.Services.AddressService -> Test.Backend.Dependencies<br>
Test.Backend.Services.ProductService -> Test.Backend.Dependencies<br>
Test.Backend.Services.UserService -> Test.Backend.Dependencies<br>
Test.Backend.Services.OrderService -> Test.Backend.Dependencies<br>

## Usage
1. The docker-compose file is present in the root of the project to allow Kafka to work locally.<br>
   The kafka_ui port is configured on 8084, if there are problems it is necessary to change it with an unused port.
2. If it is necessary to install Ubuntu and Docker I leave the following installation and configuration links:<br>
	a. Ubuntu: https://canonical-ubuntu-wsl.readthedocs-hosted.com/en/latest/guides/install-ubuntu-wsl2/<br>
	b. Docker: https://docs.docker.com/engine/install/ubuntu/<br>
	c. Docker PostInstall: https://docs.docker.com/engine/install/linux-postinstall/<br>
3. Once Docker is configured you need to start the services with the command:<br>
	 **sudo service docker start**
4. Next it needs to run the docker-docker-compose.yml file with the command:<br>
	**docker-compose -f '{pathFile}/docker-compose.yml' up --build -d**<br>
   Where {pathFile} should be replaced with the physical path of the file.<br>
   Example path file: **docker-compose -f '/mnt/c/Users/mdeidda/Documents/Visual Studio 2022/Projects/test-backend/docker-compose.yml' up --build -d**
5. Open the solution **test-backend.sln**
6. In the **appsettings.json** and **appsettings.Development.json** of each microservice add your own connectionString to connect to SqlServer.<br>
   The projects in which you will need to change the connectionString are as follows:<br>
		a. **Test.Backend.Services.AddressService**<br>
		b. **Test.Backend.Services.OrderService**<br>
		c. **Test.Backend.Services.ProductService**<br>
		d. **Test.Backend.Services.UserService**<br>
	If you run the project in Debug, you can only modify the file **appsettings.Development.json**.<br>
	It is recommended to only change the Server name of the SqlServer instance without changing the database name.
7. Startup projects should already be configured, verify in Solution Properties that WebApi projects and all microservices are set as Startup projects.<br>
   If they need to be configured, select the **Multiple startup projects** item and set the **Action** to **Start** for the following projects:<br>
		a. **Test.Backend.WebApi**<br>
		b. **Test.Backend.Services.AddressService**<br>
		c. **Test.Backend.Services.OrderService**<br>
		d. **Test.Backend.Services.ProductService**<br>
		e. **Test.Backend.Services.UserService**<br>
	Then apply the changes and click OK.
8. Start projects with VisualStudio's Start button.
9. The migration scripts should already be set up in the project and automatically run at startup.<br>
   If databases are not created when starting and opening all projects, you need to launch the **Add-Migration InitialCreate** and then **Update-Database** commands on the Package Manager Console.<br>
   These commands will need to be run for all microservices projects:<br>
		a. **Test.Backend.Services.AddressService**<br>
		b. **Test.Backend.Services.OrderService**<br>
		c. **Test.Backend.Services.ProductService**<br>
		d. **Test.Backend.Services.UserService**<br>
10. If there are Kafka errors on the consoles when starting the projects, perform the following steps:<br>
		a. Stop VisualStudio projects.<br>
		b. Execute this command to restart docker service:<br>
			**sudo service docker restart**<br>
		c. Execute this command to stop container:<br>
			**docker-compose -f '{pathFile}/docker-compose.yml' down -v**<br>
		d. Execute this command to start container :<br>
			**docker-compose -f '{pathFile}/docker-compose.yml' up --build -d**<br>
		e. Start VisualStudio projects again and verify that the error has not recurred.<br>
11. Once the projects are all open, verify that the project URLs and ports match within the **appsettings.json** and **appsettings.Development.json** files of the **Test.Backend.Services.OrderService** project.<br>
	If they differ, you are asked to stop the projects, update the **BaseAddresses** fields for all **Clients** in **HttpRefitPollyOptions** of all the endpoints and restart the project.
12. The controllers are exposed from the swagger page of the WebApi project.<br>
	To generate an order it is necessary to create all the other entities first.<br>
	Sequence example:<br>
		a. Create Category<br>
		b. Create Product with specific Category<br>
		c. Create User<br>
		d. Create Address<br>
		e. Create Order with Products, User and Address<br>

## License
2024 - Copyright (c) All rights reserved.