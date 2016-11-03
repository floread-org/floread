//Initialize p
int photoResistor = 0;    
//Input of photoResistor from sensor
int photoOutput;     
 
void setup(void) {
  // Set baud rate to 9600
  Serial.begin(9600);   
}
 
void loop(void) {
  //Read from analog input A0 on board
  photoOutput = analogRead(photoResistor);  

  //Display reading
  Serial.println("Sensor Reading = ");
  Serial.println(photoOutput);

  //Set dealy to 1 second
  delay(100);
}
