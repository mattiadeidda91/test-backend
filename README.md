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

Test.Backend.Database -> ShoppingCart.Abstractions<br>
Test.Backend.HttpClient -> ShoppingCart.Abstractions<br>
Test.Backend.Kafka -> ShoppingCart.Abstractions<br>

Test.Backend.Dependencies -> ShoppingCart.Database<br>
Test.Backend.Dependencies -> ShoppingCart.HttpClient<br>
Test.Backend.Dependencies -> ShoppingCart.Kafka<br>

Test.Backend.WebApi -> ShoppingCart.Dependencies<br>

Test.Backend.Services.AddressService -> ShoppingCart.Dependencies<br>
Test.Backend.Services.ProductService -> ShoppingCart.Dependencies<br>
Test.Backend.Services.UserService -> ShoppingCart.Dependencies<br>
Test.Backend.Services.OrderService -> ShoppingCart.Dependencies<br>

## Usage
1. The docker-compose file is present in the root of the project to allow Kafka to work locally.<br>
   The kafka_ui port is configured on 8084, if there are problems you are asked to change it to a port not in use
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
6. In the **appsettings.json** and **appsettings.Development.json** of each microservice add your own connectionString to connect to SqlServer.
7. Startup projects should already be configured, verify in Solution Properties that WebApi projects and all microservices are set as Startup projects.
8. The migration scripts should already be set up in the project and automatically run at startup.<br>
   If this does not happen, you need to launch the **Add-Migration InitialCreate** and then **Update-Database** commands on the Package Manager Console.<br>
   These commands will need to be run for all microservices projects.
9. Start projects with VisualStudio's Start button.
10. The controllers are exposed from the swagger page of the WebApi project.<br>
	To generate an order it is necessary to create all the other entities first.<br>
	Sequence example:<br>
		a. Create Category<br>
		b. Create Product with specific Category<br>
		c. Create User<br>
		d. Create Address<br>
		e. Create Order with Products, User and Address<br>

## License
2024 - Copyright (c) All rights reserved.