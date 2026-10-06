# Urnings Algorithm

The Urnings algorithm is an adaptive algorithm that tracks ratings of objects through the use of Red and Green balls. We can imagine as a mental model that these balls are stored inside of an urn (hence the name Urnings algorithm). When two objects are placed in a "match" against each other, the amount of red and green balls may shift (though the total will always remain the same). Then, through the amount of green balls an object has, a rating can be derived. In this document, Urning will be used to refer to the rating of an Object, where the Urning is defined as the ratio of green balls inside of the urn. 

Concretely for our learning platform, the objects that will be placed in "matches" are the User and the Items they have to answer inside of a level. The amount of green balls the user has represent their knowledge inside of a certain domain. The amount of green balls an Item has represents its difficulty rating. The amount of balls inside of urn is a measure for how fast (slow) a rating changes. Usually, Users will have a small urn, whereas Items should have larger urns. 

## Update rules of the algorithm
Whenever a User answers an Item, the outcome of the "match" will be Correct or Incorrect. Both the User and the Item also have an urn representing their rating. We compare the actual outcome of the match with what we would expect from the urn. 

The prediction is calculated as follows: 
```
Generate random number 0 <= x < urnTotal
If x < (#Green balls) Then (Predict Green)
Else (Predict Red)
```

We then create predictions until the prediction of the Item differs from the prediction of the User. We then predict that the "winner" of the match is the Object that drew the green ball. This means we predict the User answers correctly if it drew Green, and that it answers incorrectly if it drew red. 

Finally, we update the urns based on how our prediction relates to the real outcome. 
- If the prediction matches the real outcome, we don't need to change nothing. 
- If we predict the User to answer correctly but they are wrong, the User converts a green ball into a red ball and the Item converts a red ball into a green ball. 
- If we predict the User to answer incorrectly but they are right, the User converts a red ball into a green ball and the Item converts a green ball into a red ball. 

For Users, this heuristically means that surprising correct answers raise the rating, and surprising incorrect answers lower the rating. For Items, this heuristically means that surprising correct answers lower the difficulty, and surprising incorrect answers raise the difficulty. 

## Initial urn size
The initial urn size is the hyperparameter that tunes our adaptive algorihtm. For small urns, changes propagate quickly and ratings change fast. This is desirable for users, as they usually only interact with a certain domain for a short time. Then, their rating can change quickly and they don't have to spend too much time on a given domain. 

For items, a lot of different users will be interacting with the same Item. Thus, the urn size should be higher, so that individual Users don't skew the Item rating too quickly. 

For a first implementation, we can fix these values. Later on, it might be worth investigating if at least the Item urn size should be dynamic based on the average amount of users that do this item within a certain timeframe. 

## Variance of rating
One nice property of the Urnings algorithm is that the rating of an Object has a known variance. Because of this, we can define a mastery for an Object based on how certain we are of the rating of a player. Say we know a player has a rating of over 80% with more than 90% certainty. Then that gives a more adaptive measure for the mastery a user has of a certain domain. 

For items, this known variance can give us a good idea of anomalies. When a result significantly deviates from what we would expect to happen, this can easily be flagged so that researchers can look into it. 

## Matchmaking
the Urning can also be used to create matchmaking. Users will then be matched with Items that match their skill level. This way, users will always be ensured that they are sufficienctly engaged with the material. 

## Metropolis-Hastings
In case we use matchmaking, there is a bias inherent to our computation. This is because the act of matchmaking players with similar skill makes it more likely that ratings are changed in a way that the Item and User won't be matched again. (Because of similar urnings, they are likely to drift apart and the prediction will be most likely to be wrong compared to non-rating based matchmaking). 

This problem can be circumvented using a Metropolis-Hastings (MH) step inside of the final change. We only accept the change of urnings with a certain probability (see articles by Cito) based on if the change makes it more or less likely that the same match is made again. This step ensures that if an update makes a replay between item and user more likely (notwithstanding the fact that we limit rematches externally). If it doesn't make replays more likely, we accept the change with a probability that depends on the liklihood of a replay. 
