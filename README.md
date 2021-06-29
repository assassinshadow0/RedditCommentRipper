# RedditCommentRipper
Extract comments based on tags from reddit for machine learning purposes.

# Reddit Comment Ripper

Reddit Comment Ripper is a C# unity application for extracting comments from Reddit posts. The tool is relatively customizable. You can add words you are interested in along with words to ignore in a post.

## Installation

Run the executable in the executable directory or import the the project into unity. The tool was developed using unity 2019.4.11f1. But also tested on 2021.1.0f1 post upload to git so imports should still work.

## Usage

<img src="https://raw.githubusercontent.com/assassinshadow0/RedditCommentRipper/master/Capture.jpg">

<b>Source URL</b>  is the subreddit you want to extract comments from.

Tap <b>Tags to accept</b> or <b>Tags to Reject</b> to view or modify the tags. Then tap <b>Add a new Tag</b> to add new tags. 

In <b>Messages</b> field, progress is outputted in text form. i.e. extracting comments, or writing to file etc.

Lastly, <b>Min Chars</b> and <b>Max Chars</b> helps you define the acceptable range of characters in a sentence. Putting zero in them will make the tool pick default values for them which is min - atleast 3 characters long and max - at most 300 characters long.

Program in Action:

<img src="https://raw.githubusercontent.com/assassinshadow0/RedditCommentRipper/master/cap2.jpg">

## Output
Your output will look something like this
```output

look: 
You look like a better version of Katniss everdeen. 

look: 
Both looking amazing! 

happy: 
Very nice!!!  Happy early bday!!  And CONGRATS!!! 

look: 
You rock that look! 

look: 
You look radiant!! 

animation: 
This animation is out of the world! Can't believe an indie dev made this 

```
The file will be saved in:
C:/Users/%user name%/AppData/LocalLow/AssassinShadow/RedditCommentRipper/comments.txt
## Background
Why this?

While thinking about new machine learning project ideas, I came up with an idea: Wouldn't it be interesting if there was an app/web service that gives you a compliment from time to time? Thus, I started looking for data sets that contained compliments/selfie compliments yet could not find one. As a result, built something that would create a data set for me and then decided to share it with the world. 

In a sense, it is a bot, training another bot.
## Contributing
Pull requests are welcome.

## check out the results of this tool
https://selfieai.live/

## License
[MIT](https://choosealicense.com/licenses/mit/)
