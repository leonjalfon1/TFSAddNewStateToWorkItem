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

<b>AddNewStateToWorkItem</b> {user} {password} {collectionUrl} {teamProjects} {workItem} {newState} {transitions} {category} {stateType}    
  
<b>{user}</b>: "Domain\UserName" 
   
<b>{password}</b>: "Password"  
   
<b>{collectionUrl}</b>: "http://TfsUrl:8080/tfs/DefaultCollection"  
   
<b>{teamProjects}</b>: Team project names separetad by ',' ("tp1,tp2,tp3") or ("*") for all  
   
<b>{workItem}</b>: Workitem Name (for example: Bug)  
   
<b>{newState}</b>: New State to add (for example: Open)  
   
<b>{transitions}</b>: You can add several transitions and several reasons to the each transition separating them by ';'  
--> Transition Syntax -> {direction}:{state}({reasons})  
----> {direction}: from/to (from which state or for which state)  
----> {state}: source state (for from) or target state (for to)  
----> {reasons}: Several reasons can be added separating them by ',' (the first reason will be set as default reason)  
--> For example: "from:New(Created);to:Active(Accepted,In Progress)"  
      
<b>{category}</b>: In which category the states should be added in the ProcessConfig  
   
<b>{stateType}</b>: Attribute "value" for the element State in the ProcessConfig (Proposed,In Progress,Completed)  
   
.     

<b>For Example:</b>
 AddNewStateToWorkItem "Domain\UserName", "Password", "http://TfsUrl:8080/tfs/DefaultCollection", "*", "Bug", "Open", "from:New(default,reason1,reason2);to:Active(defaultreason,reason4)", "In Progress"
   
<b>Note:</b> The tool trusts you so doesn't do any validation (Only counts the number of parameters)

#Build
To build the application just open the solution in visual studio, make sure the references are not broken and build it.  
Third party references are stored in the folder "References".  

#Contribute
As I said earlier is a very basic project whose function is to be used as a starting point or used as a reference.  
However feel free to contribute (just fork and open a pull request), your contributions can help others :)  
  
