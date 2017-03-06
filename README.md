# TFSAddNewStateToWorkItem
Basic tool for add an state to workitems in multiple Team Projects programmatically

#Introduction 
What are you supposed to do when you want to add a status to a certain workitem on hundreds of different projects at the same time?  
There are those who will say "Be patient to manually edit all XMLs", but others (like me) will say "Let's program".  
This is a very basic tool but it can be useful as a starting point or as a reference when creating your own.  
  
#Workflow
TFSAddNewStateToWorkItem perform the following steps for each specified team project:    
1.	Export the XML for the specified workitem  
2.	Add the requested state  
3.	Add the requested transitions  
4.	Import the updated workitem XML 
5.	Export the ProcessConfig file  
6.  Add the requested state to the specified category  
7.  Import the updated ProcessConfig file    

#Getting Started
To use the tool, just build it, open the cmd and run it:  

AddNewStateToWorkItem <user> <password> <collectionUrl> <teamProjects> <workItem> <newState> <transitions> <category> <stateType>  

<user>: "Domain\UserName"
<password>: "Password"
<collectionUrl>: "http://TfsUrl:8080/tfs/DefaultCollection"
<teamProjects>: Team project names separetad by ',' ("tp1,tp2,tp3") or ("*") for all
<workItem>: Workitem Name (for example: Bug)
<newState>: New State to add (for example: Opem)
<transitions>: You can add several transition and several reasons to the new state using the following syntax
<category>:
<stateType>:

<user> <password> <collectionUrl> <teamProjects> <workItem> <newState> <transition(from:State(defaultreason,reason...);to:State(defaultreason,reason...))> <category> <stateType(Proposed,In Progress,Completed>

#Build
To build the application just open the solution in visual studio, make sure the references are not broken and build it.  
Third party references are stored in the folder "References".  

#Contribute
As I said earlier is a very basic project whose function is to be used as a starting point or used as a reference.  
However feel free to contribute (just fork and open a pull request), your contributions can help others :)  
  
